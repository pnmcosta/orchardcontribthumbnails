using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Contrib.Thumbnails
{
    public class Routes : IRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
            {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                new RouteDescriptor {
                    Priority = 20,
                    Name = "Thumbnails",
                    Route = new Route("Thumbnails",
                                      new RouteValueDictionary {
                                          {"area", "Contrib.Thumbnails"},
                                          {"controller", "Thumbnails"},
                                          {"action", "Create"}
                                      },
                                      new RouteValueDictionary() {},
                                      new RouteValueDictionary {
                                          {"area", "Contrib.Thumbnails"}
                                      },
                                      new MvcRouteHandler())
                }
            };
        }
    }
}