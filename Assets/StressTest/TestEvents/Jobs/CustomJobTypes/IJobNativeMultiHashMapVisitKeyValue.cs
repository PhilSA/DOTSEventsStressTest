using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

[JobProducerType(typeof(JobNativeMultiHashMapVisitKeyValue.JobNativeMultiHashMapVisitKeyValueProducer<,,>))]
public interface IJobNativeMultiHashMapVisitKeyValue<TKey, TValue>
    where TKey : struct, IEquatable<TKey>
    where TValue : struct
{
    void ExecuteNext(TKey key, TValue value);
}

public static class JobNativeMultiHashMapVisitKeyValue
{
    public static unsafe JobHandle ScheduleParallel<TJob, TKey, TValue>(
        this TJob jobData,
        NativeMultiHashMap<TKey, TValue> hashMap,
        int minIndicesPerJobCount,
        JobHandle dependsOn = default)
        where TJob : struct, IJobNativeMultiHashMapVisitKeyValue<TKey, TValue>
        where TKey : struct, IEquatable<TKey>
        where TValue : struct
    {
        var jobProducer = new JobNativeMultiHashMapVisitKeyValueProducer<TJob, TKey, TValue>
        {
            HashMap = hashMap,
            JobData = jobData,
        };

        var scheduleParams = new JobsUtility.JobScheduleParameters(
            UnsafeUtility.AddressOf(ref jobProducer),
            JobNativeMultiHashMapVisitKeyValueProducer<TJob, TKey, TValue>.Initialize(),
            dependsOn,
            ScheduleMode.Parallel);

        return JobsUtility.ScheduleParallelFor(ref scheduleParams, hashMap.GetUnsafeBucketData().bucketCapacityMask + 1, minIndicesPerJobCount);
    }

    internal struct JobNativeMultiHashMapVisitKeyValueProducer<TJob, TKey, TValue>
        where TJob : struct, IJobNativeMultiHashMapVisitKeyValue<TKey, TValue>
        where TKey : struct, IEquatable<TKey>
        where TValue : struct
    {
        [ReadOnly]
        public NativeMultiHashMap<TKey, TValue> HashMap;

        internal TJob JobData;

        private static IntPtr jobReflectionData;

        private delegate void ExecuteJobFunction(
            ref JobNativeMultiHashMapVisitKeyValueProducer<TJob, TKey, TValue> producer,
            IntPtr additionalPtr,
            IntPtr bufferRangePatchData,
            ref JobRanges ranges,
            int jobIndex);

        internal static IntPtr Initialize()
        {
            if (jobReflectionData == IntPtr.Zero)
            {
                jobReflectionData = JobsUtility.CreateJobReflectionData(
                    typeof(JobNativeMultiHashMapVisitKeyValueProducer<TJob, TKey, TValue>),
                    typeof(TJob),
                    (ExecuteJobFunction)Execute);
            }

            return jobReflectionData;
        }

        internal static unsafe void Execute(
            ref JobNativeMultiHashMapVisitKeyValueProducer<TJob, TKey, TValue> fullData,
            IntPtr additionalPtr,
            IntPtr bufferRangePatchData,
            ref JobRanges ranges,
            int jobIndex)
        {
            while (true)
            {
                if (!JobsUtility.GetWorkStealingRange(ref ranges, jobIndex, out var begin, out var end))
                {
                    return;
                }

                UnsafeHashMapBucketData bucketData = fullData.HashMap.GetUnsafeBucketData();
                int* buckets = (int*)bucketData.buckets;
                int* nextPtrs = (int*)bucketData.next;
                byte* keys = bucketData.keys;
                byte* values = bucketData.values;

                for (int i = begin; i < end; i++)
                {
                    int entryIndex = buckets[i];

                    while (entryIndex != -1)
                    {
                        TKey key = UnsafeUtility.ReadArrayElement<TKey>(keys, entryIndex);
                        TValue value = UnsafeUtility.ReadArrayElement<TValue>(values, entryIndex);

                        fullData.JobData.ExecuteNext(key, value);

                        entryIndex = nextPtrs[entryIndex];
                    }
                }
            }
        }
    }
}