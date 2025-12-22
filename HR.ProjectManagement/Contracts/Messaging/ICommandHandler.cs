

namespace HR.ProjectManagement.Contracts.Messaging;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<Results> Handle(TCommand request, CancellationToken cancellationToken);
}

// Handler for ICommand<TResponse>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<Results<TResponse>> Handle(TCommand request, CancellationToken cancellationToken);
}