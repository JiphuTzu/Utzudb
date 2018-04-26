using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using strange.extensions.mediation.api;
using strange.extensions.mediation.impl;
using strange.extensions.reflector.api;
using strange.extensions.signal.impl;
//============================================================
//@author	JiphuTzu
//@create	08/31/2017
//@company	FaymAR
//
//@description:
//============================================================
namespace Faymar.Contexts
{
    public class SignalViewBinder : AbstractMediationBinder
    {
        public SignalViewBinder() {}
        public override void Trigger(MediationEvent evt, IView view)
        {
            if (evt == MediationEvent.AWAKE)
            {
                InjectViewAndChildren(view);
                HandleDelegates(view, true);
            }
            else if (evt == MediationEvent.DESTROYED)
            {
                HandleDelegates(view, false);
                injectionBinder.injector.Uninject(view);
            }
        }
        protected void HandleDelegates(IView view, bool toAdd)
        {
            //Debug.Log("Handle delegates..."+view+" >>> "+toAdd);
            IReflectedClass reflectedClass = injectionBinder.injector.reflector.Get(view.GetType());
            //GetInstance Signals and add listeners
            foreach (var pair in reflectedClass.attrMethods)
            {
                if (pair.Value is ListensTo)
                {
                    ListensTo attr = (ListensTo)pair.Value;
                    ISignal signal = (ISignal)injectionBinder.GetInstance(attr.type);
                    if (toAdd) AssignDelegate(view, signal, pair.Key);
                    else RemoveDelegate(view, signal, pair.Key);
                }
            }
        }

        /// Remove any existing ListensTo Delegates
        protected void RemoveDelegate(object view, ISignal signal, MethodInfo method)
		{
			if (signal.GetType().BaseType.IsGenericType) //e.g. Signal<T>, Signal<T,U> etc.
			{
				Delegate toRemove = Delegate.CreateDelegate(signal.listener.GetType(), view, method);
				signal.listener = Delegate.Remove(signal.listener,toRemove);
			}
			else
			{
				((Signal)signal).RemoveListener((Action)Delegate.CreateDelegate(typeof(Action), view, method)); //Assign and cast explicitly for Type == Signal case
			}
		}

		/// Apply ListensTo delegates
		protected void AssignDelegate(object view, ISignal signal, MethodInfo method)
		{
			if (signal.GetType().BaseType.IsGenericType)
			{
				var toAdd = Delegate.CreateDelegate(signal.listener.GetType(), view, method); //e.g. Signal<T>, Signal<T,U> etc.
				signal.listener = Delegate.Combine(signal.listener, toAdd);
			}
			else
			{
				((Signal)signal).AddListener((Action)Delegate.CreateDelegate(typeof(Action), view, method)); //Assign and cast explicitly for Type == Signal case
			}
		}
        protected override IView[] GetViews(IView view)
        {
            MonoBehaviour mono = view as MonoBehaviour;
            Component[] components = mono.GetComponentsInChildren(typeof(IView), true);
            return components.Cast<IView>().ToArray();
        }
        protected override bool HasMediator(IView view, Type mediatorType)
        {
            return true;
        }
        protected override object CreateMediator(IView view, Type mediatorType)
        {
            return null;
        }

        protected override IMediator DestroyMediator(IView view, Type mediatorType)
        {
            return null;
        }

        protected override object DisableMediator(IView view, Type mediatorType)
        {
            return null;
        }

        protected override object EnableMediator(IView view, Type mediatorType)
        {
            return null;
        }


        protected override void ThrowNullMediatorError(Type viewType, Type mediatorType)
        {
            throw new MediationException("The view: " + viewType.ToString() + " is mapped to mediator: "
                                         + mediatorType.ToString() + ". AddComponent resulted in null, which probably means "
                                         + mediatorType.ToString().Substring(mediatorType.ToString().LastIndexOf(".") + 1) + " is not a MonoBehaviour.",
            MediationExceptionType.NULL_MEDIATOR);
        }
    }
}