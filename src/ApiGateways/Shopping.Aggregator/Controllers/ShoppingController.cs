using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shopping.Aggregator.Models;
using Shopping.Aggregator.Services;
using System.Net;

namespace Shopping.Aggregator.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ShoppingController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICatalogService _catalogService;
        private readonly IBasketService _basketService;

        public ShoppingController(IOrderService orderService, ICatalogService catalogService, IBasketService basketService)
        {
            _orderService = orderService;
            _catalogService = catalogService;
            _basketService = basketService;
        }

        [HttpGet("{userName}", Name = "GetShopping")]
        [ProducesResponseType(typeof(ShoppingModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingModel>> GetShopping(string userName)
        {
            // get basket with userName
            var basket = await _basketService.GetBasket(userName);

            // iterate basket items and consume proucts with basket item productId member
            foreach (var basketItem in basket.Items)
            {
                var product = await _catalogService.GetCatalog(basketItem.ProductId);

                // map product related members into basket items dto with extended columns
                basketItem.ProductName = product.Name;
                basketItem.Summary = product.Summary;
                basketItem.ImageFile = product.ImageFile;
                basketItem.Description = product.Description;
                basketItem.Category = product.Category;
            }

            // consum ordering microservice in order to retreive order list 
            var orders = await _orderService.GetOrdersByUserName(userName);

            // return root ShoppingModel dto class which including all responses

            var shoppingModel = new ShoppingModel
            {
                UserName = userName,
                BasketWithProducts = basket,
                Orders = orders
            };

            return Ok(shoppingModel);
        }
    }
}
