using NUnit.Framework;

public class SettingsMenuTests
{
    [Test]
    public void HidesRuntimeButtonOnlyOnTitleScene()
    {
        Assert.IsFalse(SettingsMenu.ShouldShowButton("TitleScene"));
        Assert.IsTrue(SettingsMenu.ShouldShowButton("Map1"));
        Assert.IsTrue(SettingsMenu.ShouldShowButton("Tutorial"));
    }
}
