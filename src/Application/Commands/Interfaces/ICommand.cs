namespace EnvironmentsService.Src.Application.Commands.Interfaces;

public interface ICommand<TResult>
{
    Task<TResult> ExecuteAsync();
}
