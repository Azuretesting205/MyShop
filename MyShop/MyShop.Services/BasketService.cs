using MyShop.Core.Contracts;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyShop.Services
{
    public class BasketService
    {
        IRepository<Product> productContext;
        IRepository<ProductCategory> productCategoriesContext;
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
                        basket = createNewBasket(basketId);
                    }
                }
            }
            else
            {
                if (createIfNull)
                {
                    basket = createNewBasket(basketId);
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
            cookie.Value = cookie.Value;
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
    }


}
