using NUnit.Framework;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
#if !UNITY_DOTSRUNTIME
using UnityEngine.LowLevel;
#endif

namespace TestHelper
{
#if NET_DOTS
    public class EmptySystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
        }

        public new EntityQuery GetEntityQuery(params EntityQueryDesc[] queriesDesc)
        {
            return base.GetEntityQuery(queriesDesc);
        }

        public new EntityQuery GetEntityQuery(params ComponentType[] componentTypes)
        {
            return base.GetEntityQuery(componentTypes);
        }

        public new EntityQuery GetEntityQuery(NativeArray<ComponentType> componentTypes)
        {
            return base.GetEntityQuery(componentTypes);
        }

        public unsafe new BufferFromEntity<T> GetBufferFromEntity<T>(bool isReadOnly = false) where T : struct, IBufferElementData
        {
            CheckedState()->AddReaderWriter(isReadOnly ? ComponentType.ReadOnly<T>() : ComponentType.ReadWrite<T>());
            return EntityManager.GetBufferFromEntity<T>(isReadOnly);
        }
    }
#else
	public partial class EmptySystem : SystemBase
	{
		protected override void OnUpdate() {}

		public new EntityQuery GetEntityQuery(params EntityQueryDesc[] queriesDesc) => base.GetEntityQuery(queriesDesc);

		public new EntityQuery GetEntityQuery(params ComponentType[] componentTypes) => base.GetEntityQuery(componentTypes);

		public new EntityQuery GetEntityQuery(NativeArray<ComponentType> componentTypes) => base.GetEntityQuery(componentTypes);
	}

#endif

	public class ECSTestsCommonBase
	{
		[SetUp]
		public virtual void Setup()
		{
#if UNITY_DOTSRUNTIME
            Unity.Runtime.TempMemoryScope.EnterScope();
#endif
		}

		[TearDown]
		public virtual void TearDown()
		{
#if UNITY_DOTSRUNTIME
            Unity.Runtime.TempMemoryScope.ExitScope();
#endif
		}
	}

	public abstract class ECSTestsFixture : ECSTestsCommonBase
	{
		protected World m_PreviousWorld;
		protected World m_World;
#if !UNITY_DOTSRUNTIME
		protected PlayerLoopSystem m_PreviousPlayerLoop;
#endif
		protected EntityManager m_Manager;
		protected EntityManager.EntityManagerDebug m_ManagerDebug;

		protected int StressTestEntityCount = 1000;
		protected bool CreateDefaultWorld = false;
		private bool JobsDebuggerWasEnabled;

		[SetUp]
		public override void Setup()
		{
			base.Setup();

#if !UNITY_DOTSRUNTIME
			// unit tests preserve the current player loop to restore later, and start from a blank slate.
			m_PreviousPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
			PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
#endif

			m_PreviousWorld = m_World;
			m_World = World.DefaultGameObjectInjectionWorld =
				CreateDefaultWorld ? DefaultWorldInitialization.Initialize("Default Test World") : new World("Empty Test World");
			m_Manager = m_World.EntityManager;
			m_ManagerDebug = new EntityManager.EntityManagerDebug(m_Manager);

			// Many ECS tests will only pass if the Jobs Debugger enabled;
			// force it enabled for all tests, and restore the original value at teardown.
			JobsDebuggerWasEnabled = JobsUtility.JobDebuggerEnabled;
			JobsUtility.JobDebuggerEnabled = true;
#if !UNITY_DOTSRUNTIME
			//JobsUtility.ClearSystemIds();
			JobUtility_ClearSystemIds();
#endif
		}

		[TearDown]
		public override void TearDown()
		{
			if (m_World != null && m_World.IsCreated)
			{
				// Clean up systems before calling CheckInternalConsistency because we might have filters etc
				// holding on SharedComponentData making checks fail
				while (m_World.Systems.Count > 0)
					m_World.DestroySystem(m_World.Systems[0]);

				m_ManagerDebug.CheckInternalConsistency();

				m_World.Dispose();
				m_World = null;

				m_World = m_PreviousWorld;
				m_PreviousWorld = null;
				m_Manager = default;
			}

			JobsUtility.JobDebuggerEnabled = JobsDebuggerWasEnabled;
#if !UNITY_DOTSRUNTIME
			//JobsUtility.ClearSystemIds();
			JobUtility_ClearSystemIds();
#endif

#if !UNITY_DOTSRUNTIME
			PlayerLoop.SetPlayerLoop(m_PreviousPlayerLoop);
#endif

			base.TearDown();
		}

		// calls JobUtility internal method
		private void JobUtility_ClearSystemIds() =>
			typeof(JobsUtility).GetMethod("ClearSystemIds", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
	}
}