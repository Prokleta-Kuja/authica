using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace authica.Extensions;

public static class IJSRuntimeExtensions
{
    public static ValueTask NavigateBack(this IJSRuntime js) => js.InvokeVoidAsync("window.history.back");
}