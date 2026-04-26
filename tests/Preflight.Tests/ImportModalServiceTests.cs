using Preflight.App.Layout;
using Preflight.App.Services;

namespace Preflight.Tests;

public sealed class ImportModalServiceTests
{
    [Fact]
    public void Open_ThenClose_ClearsHandler_AndRaisesStateChanged()
    {
        var sut = new ImportModalService();
        var stateChangedCount = 0;
        sut.StateChanged += () => stateChangedCount++;

        sut.Open(_ => Task.CompletedTask);

        Assert.True(sut.IsOpen);
        Assert.NotNull(sut.ResultHandler);
        Assert.Equal(1, stateChangedCount);

        sut.Close();

        Assert.False(sut.IsOpen);
        Assert.Null(sut.ResultHandler);
        Assert.Equal(2, stateChangedCount);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_DoesNotRaiseStateChanged()
    {
        var sut = new ImportModalService();
        var stateChangedCount = 0;
        sut.StateChanged += () => stateChangedCount++;

        sut.Close();

        Assert.Equal(0, stateChangedCount);
        Assert.False(sut.IsOpen);
        Assert.Null(sut.ResultHandler);
    }
}
