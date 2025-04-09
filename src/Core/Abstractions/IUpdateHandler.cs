using WTelegram.Types;

namespace Himawari.Core.Abstractions;

public interface IUpdateHandler
{ 
    Task OnUpdate(Update arg);
}