using MyShop.Core.Contracts;
using MyShop.Core.Models;
using MyShop.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyShop.Services
{
    public class BasketService : IBasketService
    {
        IRepository<Product> productContext;
        IRepository<Basket> basketContext;

        public const string BasketSessionName = "eCommerceBasket";

        public BasketService (IRepository<Product> Context,
        IRepository<Basket> BasketContext)
        {
            this.productContext = Context;
            this.basketContext = BasketContext;
        }

        private Basket GetBasket(HttpContextBase httpContext , bool createIfNull)
        {
            HttpCookie cookie = httpContext.Request.Cookies.Get(BasketSessionName);

            Basket basket = new Basket();

            if(cookie != null)
            {
                string basketId = cookie.Value;
                if(!string.IsNullOrEmpty(basketId))
                {
                    basket = basketContext.Find(basketId);
                }
                else
                {
                    if(createIfNull)
                    {
                        basket = createNewBasket(httpContext);
                    }
                }
            }
            else
            {
                if (createIfNull)
                {
                    basket = createNewBasket(httpContext);
                }
            }

            return basket;
        }


        private Basket createNewBasket(HttpContextBase httpContext)
        { 
            Basket basket = new Basket();
            basketContext.Insert(basket);
            basketContext.Commit();

            HttpCookie cookie = new HttpCookie(BasketSessionName);
            cookie.Value = basket.Id;
            cookie.Expires = DateTime.Now.AddDays(1);
            httpContext.Response.Cookies.Add(cookie);

            return basket;
        }

        public void AddToBasket(HttpContextBase httpContext, string productId) 
        {
            Basket basket = GetBasket(httpContext, true);
            BasketItem basketItem = basket.BasketItems.FirstOrDefault(i => i.ProductId == productId);

            if(basketItem==null)
            {
                basketItem = new BasketItem { BasketId = basket.Id, ProductId = productId, Quantity = 1 };
                basket.BasketItems.Add(basketItem);
            }
            else
            {
                basketItem.Quantity = basketItem.Quantity + 1;
            }

            basketContext.Commit();
        }

        public void RemoveFromBasket(HttpContextBase httpContext, string itemId)
        {
            Basket basket = GetBasket(httpContext, true);
            BasketItem basketItem = basket.BasketItems.FirstOrDefault(i => i.Id == itemId);

            if (basketItem != null)
            {                
                basket.BasketItems.Remove(basketItem);
            }

            basketContext.Commit();
        }

        public List<BasketItemViewModel> GetBasketItems(HttpContextBase httpContext)
        {
            Basket basket = GetBasket(httpContext,false);
            if(basket != null)
            {
                var result = (from b in basket.BasketItems
                              join p in productContext.Collection() on b.ProductId equals p.Id
                              select new BasketItemViewModel()
                              {
                                  Id = b.Id,
                                  ProductName = p.Name,
                                  Image = p.Image,
                                  Price = p.Price,
                                  Quantity = b.Quantity
                              }).ToList();

                return result;
            }
            else
            {
                return new List<BasketItemViewModel>();
            }
            
        }

        public BasketSummaryViewModel GetBasketSummary(HttpContextBase httpContext)
        {
            Basket basket = GetBasket(httpContext, false);
            BasketSummaryViewModel model = new BasketSummaryViewModel(0, 0);

            if(basket!=null)
            {
                int? basketCount = (from b in basket.BasketItems
                                    select b.Quantity).Sum();

                decimal? basketTotal = (from b in basket.BasketItems
                                        join p in productContext.Collection() on
                                        b.ProductId equals p.Id
                                        select b.Quantity * p.Price).Sum();

                model.BasketCount = basketCount ?? 0;
                model.BasketTotal = basketTotal ?? decimal.Zero;

                return model;
            }
            else
            {
                return model;
            }
        }
    }


}
