using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;
using SfsExtras.Extensions;

namespace SfsExtras.Base
{
    public abstract class SeleniumBasePage
    {
        public RemoteWebDriver Driver { get; set; }

        public Actions Actions { get; set; }

        public void InitElements()
        {
            PageFactory.InitElements(Driver, this);
        }

        public virtual List<By> GetPageIdentitySelectors()
        {
            return GetType()
                .GetProperties()
                .Select(p => p.GetFirstDecoration<FindsByAttribute>())
                .Where(a =>
                    ((object)a) != null
                    && a != null)
                .Select(Utils.ByFactory.From)
                .ToList();
        }

        public By SelectorFor<TPage, TProperty>(TPage page, Expression<Func<TPage, TProperty>> expression)
        {
            var attribute = Extensions.ReflectionExtensions.ResolveMember(page, expression).GetFirstDecoration<FindsByAttribute>();
            return Utils.ByFactory.From(attribute);
        }
    }
}
