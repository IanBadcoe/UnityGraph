using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Generation.G;
using System;
using Assets.Generation.U;
using Assets.Generation.G.GLInterfaces;
using Assets.Generation.Templates;

public class TemplateStoreTest
{

    [Test]
    public void TestAddTemplate()
    {
        TemplateStore ts = new TemplateStore();

        TemplateBuilder tb1 = new TemplateBuilder("a", "");
        TemplateBuilder tb2 = new TemplateBuilder("b", "");
        TemplateBuilder tb1a = new TemplateBuilder("a", "");

        Template t1 = tb1.Build();
        Template t2 = tb2.Build();
        Template t1a = tb1a.Build();

        Assert.AreEqual(0, ts.NumTemplates());

        Assert.IsTrue(ts.AddTemplate(t1));
        Assert.AreEqual(1, ts.NumTemplates());

        // can't add same one again
        Assert.IsFalse(ts.AddTemplate(t1));
        Assert.AreEqual(1, ts.NumTemplates());

        // can't add one with same name
        Assert.IsFalse(ts.AddTemplate(t1a));
        Assert.AreEqual(1, ts.NumTemplates());

        Assert.IsTrue(ts.AddTemplate(t2));
        Assert.AreEqual(2, ts.NumTemplates());
    }

    [Test]
    public void TestGetTemplatesCopy()
    {
        TemplateStore ts = new TemplateStore();

        TemplateBuilder tb1 = new TemplateBuilder("a", "");
        TemplateBuilder tb2 = new TemplateBuilder("b", "");

        Template t1 = tb1.Build();
        Template t2 = tb2.Build();

        ts.AddTemplate(t1);
        ts.AddTemplate(t2);

        List<Template> copy = ts.GetTemplatesCopy();

        Assert.AreEqual(2, copy.Count);
        Assert.IsTrue(copy.Contains(t1));
        Assert.IsTrue(copy.Contains(t2));

        // master list not changed by editing copy...
        copy.Clear();
        Assert.AreEqual(2, ts.NumTemplates());
    }

    [Test]
    public void TestFindByName()
    {
        TemplateStore ts = new TemplateStore();

        TemplateBuilder tb1 = new TemplateBuilder("a", "");
        TemplateBuilder tb2 = new TemplateBuilder("b", "");

        Template t1 = tb1.Build();
        Template t2 = tb2.Build();

        ts.AddTemplate(t1);
        ts.AddTemplate(t2);

        Assert.AreEqual(t1, ts.FindByName("a"));
        Assert.AreEqual(t2, ts.FindByName("b"));
        Assert.IsNull(ts.FindByName("Richard of York"));
    }

    [Test]
    public void TestContains()
    {
        TemplateStore ts = new TemplateStore();

        TemplateBuilder tb1 = new TemplateBuilder("a", "");
        TemplateBuilder tb2 = new TemplateBuilder("b", "");

        Template t1 = tb1.Build();
        Template t2 = tb2.Build();

        ts.AddTemplate(t1);
        ts.AddTemplate(t2);

        Assert.IsTrue(ts.Contains("a"));
        Assert.IsTrue(ts.Contains("b"));
        Assert.IsFalse(ts.Contains("Ambivalent Bob"));
    }
}