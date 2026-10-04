using Carnitas.Tree;
using Sarsoo.Terraform.JsonOutput.Plan;

namespace Carnitas.Test;

public class TerraformAddressTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseModulePath_returns_empty_for_missing_address(string? moduleAddress)
    {
        Assert.Empty(TerraformAddress.ParseModulePath(moduleAddress));
    }

    [Fact]
    public void ParseModulePath_handles_single_module()
    {
        Assert.Equal(["module.foo"], TerraformAddress.ParseModulePath("module.foo"));
    }

    [Fact]
    public void ParseModulePath_handles_nested_modules()
    {
        Assert.Equal(
            ["module.foo", "module.bar"],
            TerraformAddress.ParseModulePath("module.foo.module.bar"));
    }

    [Fact]
    public void ParseModulePath_preserves_instance_keys_including_dots()
    {
        Assert.Equal(
            ["module.foo[0]", "module.bar[\"k.y\"]", "module.baz[\"a]b\"]"],
            TerraformAddress.ParseModulePath("module.foo[0].module.bar[\"k.y\"].module.baz[\"a]b\"]"));
    }

    [Fact]
    public void ParseModulePath_handles_escaped_quotes_in_keys()
    {
        Assert.Equal(
            ["module.a[\"x\\\"y\"]"],
            TerraformAddress.ParseModulePath("module.a[\"x\\\"y\"]"));
    }

    [Fact]
    public void ParseModulePath_handles_deep_module_chain()
    {
        const string address = "module.a.module.b.module.c.module.d.module.e.module.f";

        Assert.Equal(
            ["module.a", "module.b", "module.c", "module.d", "module.e", "module.f"],
            TerraformAddress.ParseModulePath(address));
    }

    [Fact]
    public void GetRelativeAddress_returns_full_address_for_root_resource()
    {
        var change = new ResourceChange
        {
            Address = "aws_instance.web",
            ModuleAddress = null
        };

        Assert.Equal("aws_instance.web", TerraformAddress.GetRelativeAddress(change));
    }

    [Fact]
    public void GetRelativeAddress_strips_module_prefix()
    {
        var change = new ResourceChange
        {
            Address = "module.foo.aws_instance.web",
            ModuleAddress = "module.foo"
        };

        Assert.Equal("aws_instance.web", TerraformAddress.GetRelativeAddress(change));
    }

    [Fact]
    public void GetRelativeAddress_strips_nested_module_prefix()
    {
        var change = new ResourceChange
        {
            Address = "module.foo.module.bar.aws_instance.web[0]",
            ModuleAddress = "module.foo.module.bar"
        };

        Assert.Equal("aws_instance.web[0]", TerraformAddress.GetRelativeAddress(change));
    }

    [Fact]
    public void GetRelativeAddress_falls_back_to_type_name_index()
    {
        var change = new ResourceChange
        {
            Address = null!,
            Type = "aws_instance",
            Name = "web",
            Index = "0"
        };

        Assert.Equal("aws_instance.web[0]", TerraformAddress.GetRelativeAddress(change));
    }
}
