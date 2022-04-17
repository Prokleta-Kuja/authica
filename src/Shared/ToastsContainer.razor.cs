using System;
using System.Collections.Generic;
using System.Linq;
using authica.Services;
using Microsoft.AspNetCore.Components;

namespace authica.Shared;

public partial class ToastContainer : IDisposable
{
    const int MaxToastCount = 3;
    [Inject] private ToastService ToastService { get; set; } = null!;
    internal List<ToastSettings> ToastList { get; set; } = new();
    internal Queue<ToastSettings> ToastWaitingQueue { get; set; } = new();

    public void Dispose() => ToastService.OnShow -= ShowToast;
    protected override void OnInitialized()
    {
        ToastService.OnShow += ShowToast;
    }
    void RemoveToast((Guid ToastId, bool IsActionClick) arg)
    {
        var toast = ToastList.SingleOrDefault(x => x.Id == arg.ToastId);
        if (toast != null)
        {
            if (arg.IsActionClick)
                toast.OnClick?.Invoke();
            ToastList.Remove(toast);
        }

        if (ToastWaitingQueue.Any())
            ToastList.Add(ToastWaitingQueue.Dequeue());

        StateHasChanged();
    }
    void ShowToast(ToastSettings toast)
    {
        if (ToastList.Count >= MaxToastCount)
            ToastWaitingQueue.Enqueue(toast);
        else
        {
            ToastList.Add(toast);
            StateHasChanged();
        }
    }
}