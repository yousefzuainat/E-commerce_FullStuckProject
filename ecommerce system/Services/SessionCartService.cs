using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using ecommerce_system.ViewModels;

namespace ecommerce_system.Services
{
    public static class SessionCartService
    {
        private const string CartKey = "voltex_cart";

        public static List<SessionCartItem> GetCart(HttpContext context)
        {
            var json = context.Request.Cookies[CartKey];
            if (string.IsNullOrEmpty(json))
                return new List<SessionCartItem>();

            try {
                return JsonSerializer.Deserialize<List<SessionCartItem>>(json) ?? new List<SessionCartItem>();
            } catch {
                return new List<SessionCartItem>();
            }
        }

        public static void SaveCart(HttpContext context, List<SessionCartItem> cart)
        {
            var options = new CookieOptions 
            { 
                Expires = DateTimeOffset.UtcNow.AddDays(30), 
                IsEssential = true, 
                Path = "/" 
            };
            context.Response.Cookies.Append(CartKey, JsonSerializer.Serialize(cart), options);
        }

        public static void AddItem(HttpContext context, int productId, int quantity = 1)
        {
            var cart = GetCart(context);
            var existing = cart.Find(i => i.ProductId == productId);
            if (existing != null)
                existing.Quantity += quantity;
            else
                cart.Add(new SessionCartItem { ProductId = productId, Quantity = quantity });
            SaveCart(context, cart);
        }

        public static void UpdateItem(HttpContext context, int productId, int quantity)
        {
            var cart = GetCart(context);
            var item = cart.Find(i => i.ProductId == productId);
            if (item == null) return;
            if (quantity <= 0)
                cart.Remove(item);
            else
                item.Quantity = quantity;
            SaveCart(context, cart);
        }

        public static void RemoveItem(HttpContext context, int productId)
        {
            var cart = GetCart(context);
            cart.RemoveAll(i => i.ProductId == productId);
            SaveCart(context, cart);
        }

        public static void ClearCart(HttpContext context)
        {
            context.Response.Cookies.Delete(CartKey);
        }

        public static int GetTotalQuantity(HttpContext context)
        {
            return GetCart(context).Sum(i => i.Quantity);
        }
    }
}
