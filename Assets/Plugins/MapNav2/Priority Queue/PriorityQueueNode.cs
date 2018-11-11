namespace Priority_Queue
{
    public class PriorityQueueNode
    {
		/// <summary>
		/// Added for MapNav so that I do not have to derive a class just for this.
		/// Index into the MapNav grid array
		/// </summary>
		public int idx { get; set; }

        /// <summary>
        /// The Priority to insert this node at.  Must be set BEFORE adding a node to the queue
        /// </summary>
        public double Priority { get; set; }

        /// <summary>
        /// <b>Used by the priority queue - do not edit this value.</b>
        /// Represents the order the node was inserted in
        /// </summary>
        public long InsertionIndex { get; set; }

        /// <summary>
        /// <b>Used by the priority queue - do not edit this value.</b>
        /// Represents the current position in the queue
        /// </summary>
        public int QueueIndex { get; set; }
    }
}
