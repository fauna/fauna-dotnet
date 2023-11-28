using NUnit.Framework;

namespace Fauna.Test;

[TestFixture]
public class ConfigBuilderTests
{
    [Test]
    public void ConstructorWorksFine()
    {
        var b = ClientConfig.CreateBuilder().SetSecret("secret").Build();

        Assert.AreEqual("secret", b.Secret);
        Assert.AreEqual(new Uri(Constants.Endpoints.Default), b.Endpoint);
    }
}
