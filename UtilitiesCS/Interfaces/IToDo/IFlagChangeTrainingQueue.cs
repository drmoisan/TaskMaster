namespace UtilitiesCS.Interfaces
{
    public interface IFlagChangeTrainingQueue
    {
        public enum QueueOptions
        {
            Immediate,
            Timed
        }

        QueueOptions Options { get; set; }

        void Enqueue(IFlagChangeGroup item);
        IFlagChangeTrainingQueue Init();
    }
}