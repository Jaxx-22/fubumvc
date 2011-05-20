﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuMVC.Core.Urls;
using FubuMVC.Spark.Rendering;
using FubuTestingSupport;
using Microsoft.Practices.ServiceLocation;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuMVC.Spark.Tests.Rendering
{
    [TestFixture]
    public class FubuSparkViewDecoratorTester : InteractionContext<FubuSparkViewDecorator>
    {
        private IFubuSparkView _view;
        protected override void beforeEach()
        {
            _view = MockFor<IFubuSparkView>();
            _view.Stub(x => x.Content).PropertyBehavior();
            _view.Stub(x => x.OnceTable).PropertyBehavior();
            _view.Stub(x => x.Output).PropertyBehavior();
            _view.Stub(x => x.SiteResource).PropertyBehavior();
            _view.Stub(x => x.Globals).PropertyBehavior();
            _view.Stub(x => x.ElementPrefix).PropertyBehavior();
            _view.Stub(x => x.GeneratedViewId).Return(Guid.NewGuid());
            _view.Stub(x => x.ServiceLocator).PropertyBehavior();
            _view.Stub(x => x.Urls).Return(MockFor<IUrlRegistry>());
        }

        [Test]
        public void generatedviewid_is_forwarded_to_inner_view()
        {
            _view.GeneratedViewId.ShouldEqual(ClassUnderTest.GeneratedViewId);
        }

        [Test]
        public void siteresource_is_forwarded_to_inner_view()
        {
            Func<string, string> siteResource = x => "";
            ClassUnderTest.SiteResource = siteResource;
            ClassUnderTest.SiteResource
                .ShouldBeTheSameAs(_view.SiteResource)
                .ShouldBeTheSameAs(siteResource);
        }

        [Test]
        public void content_is_forwarded_to_inner_view()
        {
            var content = new Dictionary<string, TextWriter>();
            ClassUnderTest.Content = content;
            ClassUnderTest.Content
                .ShouldBeTheSameAs(_view.Content)
                .ShouldBeTheSameAs(content);
        }

        [Test]
        public void once_table_is_forwarded_to_inner_view()
        {
            var onceTable = new Dictionary<string, string>();
            ClassUnderTest.OnceTable = onceTable;
            ClassUnderTest.OnceTable
                .ShouldBeTheSameAs(_view.OnceTable)
                .ShouldBeTheSameAs(onceTable);
        }

        [Test]
        public void globals_is_forwarded_to_inner_view()
        {
            var globals = new Dictionary<string, object>();
            ClassUnderTest.Globals = globals;
            ClassUnderTest.Globals
                .ShouldBeTheSameAs(_view.Globals)
                .ShouldBeTheSameAs(globals);
        }

        [Test]
        public void output_is_forwarded_to_inner_view()
        {
            var output = new StringWriter();
            ClassUnderTest.Output = output;
            ClassUnderTest.Output
                .ShouldBeTheSameAs(_view.Output)
                .ShouldBeTheSameAs(output);
        }

        [Test]
        public void elementprefix_is_forwarded_to_inner_view()
        {
            const string elementPrefix = "fubu";
            ClassUnderTest.ElementPrefix = elementPrefix;
            ClassUnderTest.ElementPrefix
                .ShouldBeTheSameAs(_view.ElementPrefix)
                .ShouldBeTheSameAs(elementPrefix);
        }

        [Test]
        public void servicelocator_is_forwarded_to_inner_view()
        {
            var serviceLocator = MockFor<IServiceLocator>();
            ClassUnderTest.ServiceLocator = serviceLocator;
            ClassUnderTest.ServiceLocator
                .ShouldBeTheSameAs(_view.ServiceLocator)
                .ShouldBeTheSameAs(serviceLocator);
        }

        [Test]
        public void urls_is_forwarded_to_inner_view()
        {
            ClassUnderTest.Urls.ShouldBeTheSameAs(_view.Urls);
        }

        [Test]
        public void get_is_forwarded_to_inner_view()
        {
            var guid = Guid.NewGuid();
            _view.Stub(x => x.Get<string>()).Return("Hello World");
            _view.Stub(x => x.Get<Guid>()).Return(guid);
            ClassUnderTest.Get<string>().ShouldEqual("Hello World");
            ClassUnderTest.Get<Guid>().ShouldEqual(guid);
        }

        [Test]
        public void getnew_is_forwarded_to_inner_view()
        {
            var guid = Guid.NewGuid();
            _view.Stub(x => x.GetNew<string>()).Return("Hello World");
            _view.Stub(x => x.GetNew<Guid>()).Return(guid);
            ClassUnderTest.GetNew<string>().ShouldEqual("Hello World");
            ClassUnderTest.GetNew<Guid>().ShouldEqual(guid);
        }

        [Test]
        public void render_is_forwarded_to_inner_view()
        {
            _view.Expect(x => x.Render());
            ClassUnderTest.Render();
            _view.VerifyAllExpectations();
        }

        [Test]
        public void render_wraps_execution_with_pre_and_post_render_delegates()
        {
            var callStack = new List<string>();

            Action<IFubuSparkView> preRender1 = x => callStack.Add("Pre Render1");
            Action<IFubuSparkView> preRender2 = x => callStack.Add("Pre Render2");
            Action<IFubuSparkView> postRender1 = x => callStack.Add("Post Render1");
            Action<IFubuSparkView> postRender2 = x => callStack.Add("Post Render2");

            ClassUnderTest.PreRender += preRender1;
            ClassUnderTest.PreRender += preRender2;
            ClassUnderTest.PostRender += postRender1;
            ClassUnderTest.PostRender += postRender2;
            _view.Stub(x => x.Render()).WhenCalled(x => callStack.Add("Render View"));

            ClassUnderTest.Render();

            callStack.ShouldHaveCount(5);
            callStack.ElementAt(0).ShouldEqual("Pre Render1");
            callStack.ElementAt(1).ShouldEqual("Pre Render2");
            callStack.ElementAt(2).ShouldEqual("Render View");
            callStack.ElementAt(3).ShouldEqual("Post Render1");
            callStack.ElementAt(4).ShouldEqual("Post Render2");

        }


    }
}