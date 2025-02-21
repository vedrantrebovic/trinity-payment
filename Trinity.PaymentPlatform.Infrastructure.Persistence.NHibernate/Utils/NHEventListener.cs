// using MediatR;
// using NHibernate.Event;
// using Synergy.Infrastructure.Persistence.Extensions;
// using Synergy.Model.SeedWork;
//
// namespace Synergy.Infrastructure.Persistence.Utils;
//
// /// <summary>
//     /// Used to raise domain events after persistence actions are successfully completed
//     /// </summary>
//     public class NHEventListener :
//     IPostInsertEventListener,
//     IPostDeleteEventListener,
//     IPostUpdateEventListener,
//     IPostCollectionUpdateEventListener
//     {
//         private readonly IMediator _mediator;
//
//         public NHEventListener(IMediator mediator)
//         {
//             _mediator = mediator;
//         }
//
//         public void OnPostDelete(PostDeleteEvent @event)
//         {
//             DispatchEvents(@event.Entity as IEntity);
//         }
//
//         public async Task OnPostDeleteAsync(PostDeleteEvent @event, CancellationToken cancellationToken)
//         {
//             await DispatchEventsAsync(@event.Entity as IEntity);
//         }
//
//         public void OnPostInsert(PostInsertEvent @event)
//         {
//             DispatchEvents(@event.Entity as IEntity);
//         }
//
//         public async Task OnPostInsertAsync(PostInsertEvent @event, CancellationToken cancellationToken)
//         {
//             await DispatchEventsAsync(@event.Entity as IEntity);
//         }
//
//         public void OnPostUpdate(PostUpdateEvent @event)
//         {
//             DispatchEvents(@event.Entity as IEntity);
//         }
//
//         public async Task OnPostUpdateAsync(PostUpdateEvent @event, CancellationToken cancellationToken)
//         {
//             await DispatchEventsAsync(@event.Entity as IEntity);
//         }
//
//         public void OnPostUpdateCollection(PostCollectionUpdateEvent @event)
//         {
//             DispatchEvents(@event.AffectedOwnerOrNull as IEntity);
//         }
//
//         public async Task OnPostUpdateCollectionAsync(PostCollectionUpdateEvent @event, CancellationToken cancellationToken)
//         {
//             await DispatchEventsAsync(@event.AffectedOwnerOrNull as IEntity);
//         }
//
//         private void DispatchEvents(IEntity? entity)
//         {
//             if (entity == null)
//                 return;
//             _mediator.DispatchDomainEventsAsync(entity.DomainEvents).Wait();
//
//             entity.ClearDomainEvents();
//         }
//
//         private async Task DispatchEventsAsync(IEntity? entity)
//         {
//             if (entity == null)
//                 return;
//
//             await _mediator.DispatchDomainEventsAsync(entity.DomainEvents);
//             entity.ClearDomainEvents();
//         }
//     }