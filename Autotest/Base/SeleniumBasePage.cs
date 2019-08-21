﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
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

        //TODO probably should be replaced by 'GetByFor' method
        protected By SelectorFor<TPage, TProperty>(TPage page, Expression<Func<TPage, TProperty>> expression)
        {
            var attribute = Extensions.ReflectionExtensions.ResolveMember(page, expression).GetFirstDecoration<FindsByAttribute>();
            return Utils.ByFactory.From(attribute);
        }

        //Usage By selector = page.GetByFor(() => page.LoginButton);
        public By GetByFor<TProperty>(Expression<Func<TProperty>> expression)
        {
            var attribute = Extensions.ReflectionExtensions.ResolveMember(expression).GetFirstDecoration<FindsByAttribute>();
            return Utils.ByFactory.From(attribute);
        }

        //Usage By selector = page.Click(() => page.LoginButton);
        public void Click<TProperty>(Expression<Func<TProperty>> expression)
        {
            var by = GetByFor(expression);
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    Driver.FindElement(by).Click();
                    return;
                }
                catch (NoSuchElementException)
                {
                    Thread.Sleep(1000);
                }
                catch (StaleElementReferenceException)
                {
                    Thread.Sleep(1000);
                }
                catch (NullReferenceException)
                {
                    Thread.Sleep(1000);
                }
                catch (TargetInvocationException)
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
