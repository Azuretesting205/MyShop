using MyShop.Core.Models;
using MyShop.Core.ViewModels;
using MyShop.DataAccess.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyShop.WebUI.Controllers
{
    public class ProductManagerController : Controller
    {
        //ProductRepository context;
        //ProductCategoryRepository productCategoriesContext;

        InMemoryRepository<Product> context;
        InMemoryRepository<ProductCategory> productCategoriesContext;

        public ProductManagerController()
        {
            //context = new ProductRepository();
            //productCategoriesContext = new ProductCategoryRepository();
            context = new InMemoryRepository<Product>();
            productCategoriesContext = new InMemoryRepository<ProductCategory>();
        }

        // GET: ProductManager
        public ActionResult Index()
        {
            List<Product> products = context.Collection().ToList();
            return View(products);
        }

        public ActionResult Create()
        {
            ProductManagerViewModel viewModel = new ProductManagerViewModel();
            Product product = new Product();

            viewModel.product = product;
            viewModel.productCategories = productCategoriesContext.Collection().ToList();

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Create(Product product)
        {
            if(!ModelState.IsValid)
            {
                return View(product);
            }
            else
            {
                context.Insert(product);
                context.Commit();

                return RedirectToAction("Index");
            }
        }

        public ActionResult Edit(string Id)
        {
            Product product = context.Find(Id);
            if(product == null)
            {
                return HttpNotFound();
            }
            else
            {
                ProductManagerViewModel viewModel = new ProductManagerViewModel();
                viewModel.product = product;
                viewModel.productCategories = productCategoriesContext.Collection().ToList();
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Edit(Product product, string Id)
        {
            Product producttoEdit = context.Find(Id);
            if(producttoEdit == null)
            {
                return HttpNotFound();

            }
            else
            {
                if(!ModelState.IsValid)
                {
                    return View(product);
                }
                producttoEdit.Category = product.Category;
                producttoEdit.Description = product.Description;
                producttoEdit.Image = product.Image;
                producttoEdit.Name = product.Name;
                producttoEdit.Price = product.Price;

                //context.Update(producttoEdit);
                context.Commit();

                return RedirectToAction("Index");
            }
        }

        public ActionResult Delete(string Id)
        {
            Product productToDelete = context.Find(Id);
            if (productToDelete == null)
            {
                return HttpNotFound();
            }
            else
            {
                return View(productToDelete);
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult ConfirmDelete(string Id)
        {
            Product productToDelete = context.Find(Id);
            if (productToDelete == null)
            {
                return HttpNotFound();
            }
            else
            {
                context.Delete(Id);
                context.Commit();
                return RedirectToAction("Index");
            }
        }
    }

}