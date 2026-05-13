using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using ecommerce_system.ViewModels;

namespace ecommerce_system.Services
{
    public static class SessionCartService
    {
        private const string CartKey = "voltex_cart";

        public static List<SessionCartItem> GetCart(ISession session)
        {
            var json = session.GetString(CartKey);
            if (string.IsNullOrEmpty(json))
                return new List<SessionCartItem>();

            return JsonSerializer.Deserialize<List<SessionCartItem>>(json)
                   ?? new List<SessionCartItem>();
        }

        public static void SaveCart(ISession session, List<SessionCartItem> cart)
        {
            session.SetString(CartKey, JsonSerializer.Serialize(cart));
        }

        public static void AddItem(ISession session, int productId, int quantity = 1)
        {
            var cart = GetCart(session);
            var existing = cart.Find(i => i.ProductId == productId);
            if (existing != null)
                existing.Quantity += quantity;
            else
                cart.Add(new SessionCartItem { ProductId = productId, Quantity = quantity });
            SaveCart(session, cart);
        }

        public static void UpdateItem(ISession session, int productId, int quantity)
        {
            var cart = GetCart(session);
            var item = cart.Find(i => i.ProductId == productId);
            if (item == null) return;
            if (quantity <= 0)
                cart.Remove(item);
            else
                item.Quantity = quantity;
            SaveCart(session, cart);
        }

        public static void RemoveItem(ISession session, int productId)
        {
            var cart = GetCart(session);
            cart.RemoveAll(i => i.ProductId == productId);
            SaveCart(session, cart);
        }

        public static void ClearCart(ISession session)
        {
            session.Remove(CartKey);
        }

        public static int GetTotalQuantity(ISession session)
        {
            return GetCart(session).Sum(i => i.Quantity);
        }
    }
}
