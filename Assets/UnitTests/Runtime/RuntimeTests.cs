using NUnit.Framework;
using Unity.Entities;
using Unity.PerformanceTesting;
using Unity.Transforms;

/// <summary>
/// To run performance measurements:
/// - open StressTest scene
/// - open: Window => Analysis => Performance Test Report (and enable "Auto Refresh")
/// - open: Window => General => Test Runner
/// Run tests in Test Runner. Then check Performance Test Report window or the saved TestResults.xml (see Console).
/// 
/// Observations:
/// - With Burst compilation disabled, performance (and testing time) is 10 times slower! (12-core CPU)
/// - Jobs => Burst => Safety Checks => Off ... affects some tests more than others! This should be considered in summary.
/// - Jobs => Jobs Debugger ... has practically no effect on measurements
/// - Jobs => Use Job Threads ... as expected: hardly affects "Single" tests, if "off" makes companion Single/Parallel tests perform about the same 
/// </summary>
public class RuntimeTests : ECSTestsFixture
{
	// TODO: move these into a ScriptableObject for easier/faster value tweaking
	private readonly int _entityCount = 500000;
	private readonly float _spacing = 1f;
	private readonly int _damagers = 1;
	private readonly float _healthValue = 1000f;

	private void MeasureWorldUpdate(EventType eventType)
	{
		// create the HealthPrefab entity
		// NOTE: I found it difficult to verify whether this is 100% identical to the scene's Health prefab entity after conversion.
		// I add the systems from the Archetypes listed for HealthPrefab in play mode as well as the conversion code.
		// Should be identical but it's kind of hard to use the DOTS windows while tests are running whereas with breakpoints the UI is frozen.
		var healthPrefab = m_Manager.CreateEntity(typeof(LocalToWorld), typeof(Translation), typeof(Rotation), typeof(Health), typeof(Prefab));
		m_Manager.AddComponentData(healthPrefab, new Health { Value = _healthValue });
		m_Manager.AddBuffer<DamageEvent>(healthPrefab);

		// we need this to spawn all the entities during the first world update
		var spawner = m_Manager.CreateEntity(typeof(EventStressTest));
		m_Manager.SetComponentData(spawner, CreateEventStressTest(eventType, healthPrefab));

		Measure.Method(() =>
			{
				// update the world once, running all systems once
				m_World.Update();

				// this did not seem to affect measurements (by much) but it's better to be safe,
				// we don't want any jobs to continue running past the measurement cycle
				m_Manager.CompleteAllJobs();
			})
			// First update creates gazillion entities, don't measure this... (actually: could call Update outside Measure once)
			// Second update seems generally unstable, skip that too ...
			// From third run onwards measurements are stable ...
			.WarmupCount(2)
			// 10 seems enough to get a decently low deviation
			.MeasurementCount(10)
			// only measure once to keep numbers comparable to original forum post
			.IterationsPerMeasurement(1)
			.Run();
	}

	private EventStressTest CreateEventStressTest(EventType eventType, Entity prefab) => new EventStressTest
	{
		EventType = eventType,
		HealthPrefab = prefab,
		HealthEntityCount = _entityCount,
		Spacing = _spacing,
		DamagersPerHealths = _damagers,
	};

	[Test, Performance]
	public void A_ParallelWriteToStream_ParallelPollBuffers() => MeasureWorldUpdate(EventType.A_ParallelWriteToStream_ParallelPollBuffers);

	[Test, Performance]
	public void B_SingleWriteToBuffers_ParallelPollBuffers() => MeasureWorldUpdate(EventType.B_SingleWriteToBuffers_ParallelPollBuffers);

	[Test, Performance]
	public void C_ParallelWriteToBuffersECB_ParallelPollBuffers() => MeasureWorldUpdate(EventType.C_ParallelWriteToBuffersECB_ParallelPollBuffers);

	[Test, Performance]
	public void D_ParallelWriteToStream_SingleApplyToEntities() => MeasureWorldUpdate(EventType.D_ParallelWriteToStream_SingleApplyToEntities);

	[Test, Performance]
	public void E_ParallelWriteToStream_ParallelApplyToEntities() => MeasureWorldUpdate(EventType.E_ParallelWriteToStream_ParallelApplyToEntities);

	[Test, Performance]
	public void F_ParallelCreateEventEntities_SingleApplyToEntities() => MeasureWorldUpdate(EventType.F_ParallelCreateEventEntities_SingleApplyToEntities);

	[Test, Performance]
	public void G_ParallelWriteToStream_SinglePollList() => MeasureWorldUpdate(EventType.G_ParallelWriteToStream_SinglePollList);

	[Test, Performance]
	public void H_ParallelWriteToStream_SinglePollHashMap() => MeasureWorldUpdate(EventType.H_ParallelWriteToStream_SinglePollHashMap);

	[Test, Performance]
	public void I_ParallelWriteToStream_ParallelPollHashMap() => MeasureWorldUpdate(EventType.I_ParallelWriteToStream_ParallelPollHashMap);

	[Test, Performance]
	public void J_SingleDirectModification() => MeasureWorldUpdate(EventType.J_SingleDirectModification);

	public override void Setup()
	{
		// custom flag in ECSTestsFixture that allows creating the default world, rather than an empty (no systems) world
		// must be set before base.Setup() !
		CreateDefaultWorld = true;

		// setup the Entities world
		base.Setup();
	}
}