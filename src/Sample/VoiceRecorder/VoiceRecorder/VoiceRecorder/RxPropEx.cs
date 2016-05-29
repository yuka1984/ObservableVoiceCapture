using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

public static class RxPropEx
{
    public static ReadOnlyReactiveProperty<TProperty> ToReactivePropertyAsOneWaySync<TSubject, TProperty>(
        this TSubject subject,
        Expression<Func<TSubject, TProperty>> propertySelector,
        ReactivePropertyMode mode =
            ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
        bool ignoreValidationErrorValue = false)
        where TSubject : INotifyPropertyChanged
    =>
        ToReactivePropertyAsOneWaySync(subject, propertySelector, UIDispatcherScheduler.Default, mode);

    public static ReadOnlyReactiveProperty<TProperty> ToReactivePropertyAsOneWaySync<TSubject, TProperty>(
        this TSubject subject,
        Expression<Func<TSubject, TProperty>> propertySelector,
        IScheduler raiseEventScheduler,
        ReactivePropertyMode mode =
            ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
        bool ignoreValidationErrorValue = false)
        where TSubject : INotifyPropertyChanged
    {
        string propertyName; // no use

        var result = subject.ObserveProperty(propertySelector, isPushCurrentValueAtFirst: true)
            .ToReadOnlyReactiveProperty(eventScheduler: raiseEventScheduler, mode: mode);

        return result;
    }

    public static ReadOnlyReactiveProperty<TResult> ToReactivePropertyAsOneWaySync<TSubject, TProperty, TResult>(
    this TSubject subject,
    Expression<Func<TSubject, TProperty>> propertySelector
        , Func<TProperty, TResult> converter
        , ReactivePropertyMode mode =
        ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
    bool ignoreValidationErrorValue = false)
    where TSubject : INotifyPropertyChanged
=>
    ToReactivePropertyAsOneWaySync(subject, propertySelector, converter, UIDispatcherScheduler.Default, mode);

    public static ReadOnlyReactiveProperty<TResult> ToReactivePropertyAsOneWaySync<TSubject, TProperty, TResult>(
        this TSubject subject,
        Expression<Func<TSubject, TProperty>> propertySelector
        , Func<TProperty, TResult> converter
        , IScheduler raiseEventScheduler
        , ReactivePropertyMode mode =
            ReactivePropertyMode.DistinctUntilChanged | ReactivePropertyMode.RaiseLatestValueOnSubscribe,
        bool ignoreValidationErrorValue = false)
        where TSubject : INotifyPropertyChanged
    {
        string propertyName; // no use

        var result = subject.ObserveProperty(propertySelector, isPushCurrentValueAtFirst: true)
            .Select(converter)
            .ToReadOnlyReactiveProperty(eventScheduler: raiseEventScheduler, mode: mode);

        return result;
    }

    public static ReactiveProperty<TProperty> ToReactivePropertyAsOneTime<TSubject, TProperty>(
        this TSubject subject
        , Expression<Func<TSubject, TProperty>> propertySelector) where TSubject : INotifyPropertyChanged
    {
        return ReactiveProperty.FromObject(subject, propertySelector);
    }
}
