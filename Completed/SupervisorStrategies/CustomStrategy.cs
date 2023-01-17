using Akka.Actor.Internal;
using Akka.Actor;
using Akka.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinTail.SupervisorStrategies
{
    /// <summary>
    /// This class represents a fault handling strategy that applies a <see cref="Directive"/>
    /// to the single child actor that failed.
    /// </summary>
    public class CustomStrategy : SupervisorStrategy, IEquatable<CustomStrategy>
    {
        private readonly int _maxNumberOfRetries;
        private readonly int _withinTimeRangeMilliseconds;
        private readonly IDecider _decider;

        /// <summary>
        /// The number of times a child actor is allowed to be restarted, negative value means no limit,
        /// if the limit is exceeded the child actor is stopped.
        /// </summary>
        public int MaxNumberOfRetries
        {
            get { return _maxNumberOfRetries; }
        }

        /// <summary>
        /// The duration in milliseconds of the time window for <see cref="MaxNumberOfRetries"/>, negative values means no window.
        /// </summary>
        public int WithinTimeRangeMilliseconds
        {
            get { return _withinTimeRangeMilliseconds; }
        }

        /// <summary>
        /// The mapping from an <see cref="Exception"/> to <see cref="Directive"/>
        /// </summary>
        public override IDecider Decider
        {
            get { return _decider; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomStrategy"/> class.
        /// </summary>
        /// <param name="maxNrOfRetries">
        /// The number of times a child actor is allowed to be restarted, negative value means no limit,
        /// if the limit is exceeded the child actor is stopped.
        /// </param>
        /// <param name="withinTimeRange">duration of the time window for <paramref name="maxNrOfRetries"/>, <see cref="Timeout.InfiniteTimeSpan"/> means no window.</param>
        /// <param name="localOnlyDecider">The mapping used to translate an <see cref="Exception"/> to a <see cref="Directive"/>.</param>
        public CustomStrategy(int? maxNrOfRetries, TimeSpan? withinTimeRange, Func<Exception, Directive> localOnlyDecider)
            : this(maxNrOfRetries.GetValueOrDefault(-1), (int)withinTimeRange.GetValueOrDefault(Timeout.InfiniteTimeSpan).TotalMilliseconds, localOnlyDecider)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomStrategy"/> class.
        /// </summary>
        /// <param name="maxNrOfRetries">
        /// The number of times a child actor is allowed to be restarted, negative value means no limit,
        /// if the limit is exceeded the child actor is stopped.
        /// </param>
        /// <param name="withinTimeRange">duration of the time window for maxNrOfRetries, System.Threading.Timeout.InfiniteTimeSpan means no window.</param>
        /// <param name="decider">The mapping used to translate an <see cref="Exception"/> to a <see cref="Directive"/>.</param>
        public CustomStrategy(int? maxNrOfRetries, TimeSpan? withinTimeRange, IDecider decider)
            : this(maxNrOfRetries.GetValueOrDefault(-1), (int)withinTimeRange.GetValueOrDefault(Timeout.InfiniteTimeSpan).TotalMilliseconds, decider)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomStrategy"/> class.
        /// </summary>
        /// <param name="maxNrOfRetries">
        /// The number of times a child actor is allowed to be restarted, negative value means no limit,
        /// if the limit is exceeded the child actor is stopped.
        /// </param>
        /// <param name="withinTimeMilliseconds">duration in milliseconds of the time window for <paramref name="maxNrOfRetries"/>, negative values means no window.</param>
        /// <param name="localOnlyDecider">The mapping used to translate an <see cref="Exception"/> to a <see cref="Directive"/>.</param>
        /// <param name="loggingEnabled">If <c>true</c> failures will be logged</param>
        public CustomStrategy(int maxNrOfRetries, int withinTimeMilliseconds, Func<Exception, Directive> localOnlyDecider, bool loggingEnabled = true)
            : this(maxNrOfRetries, withinTimeMilliseconds, new LocalOnlyDecider(localOnlyDecider), loggingEnabled)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomStrategy"/> class.
        /// </summary>
        /// <param name="maxNrOfRetries">
        /// The number of times a child actor is allowed to be restarted, negative value means no limit,
        /// if the limit is exceeded the child actor is stopped.
        /// </param>
        /// <param name="withinTimeMilliseconds">duration in milliseconds of the time window for <paramref name="maxNrOfRetries"/>, negative values means no window.</param>
        /// <param name="decider">The mapping used to translate an <see cref="Exception"/> to a <see cref="Directive"/>.</param>
        /// <param name="loggingEnabled">If <c>true</c> failures will be logged</param>
        public CustomStrategy(int maxNrOfRetries, int withinTimeMilliseconds, IDecider decider, bool loggingEnabled = true)
        {
            _maxNumberOfRetries = maxNrOfRetries;
            _withinTimeRangeMilliseconds = withinTimeMilliseconds;
            _decider = decider;
            LoggingEnabled = loggingEnabled;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomStrategy"/> class.
        /// </summary>
        /// <param name="localOnlyDecider">mapping from Exception to <see cref="Directive" /></param>
        public CustomStrategy(Func<Exception, Directive> localOnlyDecider)
            : this(-1, -1, localOnlyDecider, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomStrategy"/> class.
        /// </summary>
        /// <param name="localOnlyDecider">The mapping used to translate an <see cref="Exception"/> to a <see cref="Directive"/>.</param>
        /// <param name="loggingEnabled">If <c>true</c> failures will be logged</param>
        public CustomStrategy(Func<Exception, Directive> localOnlyDecider, bool loggingEnabled = true)
            : this(-1, -1, localOnlyDecider, loggingEnabled)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomStrategy"/> class.
        /// </summary>
        /// <param name="decider">TBD</param>
        public CustomStrategy(IDecider decider)
            : this(-1, -1, decider, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomStrategy"/> class.
        /// </summary>
        protected CustomStrategy() : this(DefaultDecider)
        {
        }

        public CustomStrategy WithMaxNrOfRetries(int maxNrOfRetries)
        {
            return new CustomStrategy(maxNrOfRetries, _withinTimeRangeMilliseconds, _decider);
        }

        /// <summary>
        /// Determines which <see cref="Directive"/> this strategy uses to handle <paramref name="exception">exceptions</paramref>
        /// that occur in the <paramref name="child"/> actor.
        /// </summary>
        /// <param name="child">The child actor where the exception occurred.</param>
        /// <param name="exception">The exception that was thrown.</param>
        /// <returns>The directive used to handle the exception.</returns>
        protected override Directive Handle(IActorRef child, Exception exception)
        {
            return Decider.Decide(exception);
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="context">TBD</param>
        /// <param name="restart">TBD</param>
        /// <param name="child">TBD</param>
        /// <param name="cause">TBD</param>
        /// <param name="stats">TBD</param>
        /// <param name="children">TBD</param>
        public override void ProcessFailure(IActorContext context, bool restart, IActorRef child, Exception cause, ChildRestartStats stats, IReadOnlyCollection<ChildRestartStats> children)
        {

            if (restart && stats.RequestRestartPermission(MaxNumberOfRetries, WithinTimeRangeMilliseconds))
                RestartChild(child, cause, suspendFirst: false);
            else
                context.Stop(child);
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="actorContext">TBD</param>
        /// <param name="child">TBD</param>
        /// <param name="children">TBD</param>
        public override void HandleChildTerminated(IActorContext actorContext, IActorRef child, IEnumerable<IInternalActorRef> children)
        {
            throw new Exception();
            //Intentionally left blank
        }

        #region Surrogate

        /// <summary>
        /// This class represents a surrogate of a <see cref="CustomStrategy"/> router.
        /// Its main use is to help during the serialization process.
        /// </summary>
        public class CustomStrategySurrogate : ISurrogate
        {
            /// <summary>
            /// The number of times a child actor is allowed to be restarted, negative value means no limit,
            /// if the limit is exceeded the child actor is stopped.
            /// </summary>
            public int MaxNumberOfRetries { get; set; }
            /// <summary>
            /// The duration in milliseconds of the time window for <see cref="MaxNumberOfRetries"/>, negative values means no window.
            /// </summary>
            public int WithinTimeRangeMilliseconds { get; set; }
            /// <summary>
            /// The mapping from an <see cref="Exception"/> to <see cref="Directive"/>
            /// </summary>
            public IDecider Decider { get; set; }
            /// <summary>
            /// Determines if failures are logged
            /// </summary>
            public bool LoggingEnabled { get; set; }

            /// <summary>
            /// Creates a <see cref="CustomStrategy"/> encapsulated by this surrogate.
            /// </summary>
            /// <param name="system">The actor system that owns this router.</param>
            /// <returns>The <see cref="CustomStrategy"/> encapsulated by this surrogate.</returns>
            public ISurrogated FromSurrogate(ActorSystem system)
            {
                return new CustomStrategy(MaxNumberOfRetries, WithinTimeRangeMilliseconds, Decider, LoggingEnabled);
            }

        }

        /// <summary>
        /// Creates a surrogate representation of the current <see cref="CustomStrategy"/>.
        /// </summary>
        /// <param name="system">The actor system that owns this router.</param>
        /// <exception cref="NotSupportedException">This exception is thrown if the <see cref="Decider"/> is of type <see cref="LocalOnlyDecider"/>.</exception>
        /// <returns>The surrogate representation of the current <see cref="CustomStrategy"/>.</returns>
        public override ISurrogate ToSurrogate(ActorSystem system)
        {
            if (Decider is LocalOnlyDecider)
                throw new NotSupportedException("Can not serialize LocalOnlyDecider");
            return new CustomStrategySurrogate
            {
                Decider = Decider,
                LoggingEnabled = LoggingEnabled,
                MaxNumberOfRetries = MaxNumberOfRetries,
                WithinTimeRangeMilliseconds = WithinTimeRangeMilliseconds
            };
        }
        #endregion

        #region Equals

        public bool Equals(CustomStrategy other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;

            return MaxNumberOfRetries.Equals(other.MaxNumberOfRetries) &&
                   WithinTimeRangeMilliseconds.Equals(other.WithinTimeRangeMilliseconds) &&
                   Decider.Equals(other.Decider);
        }


        public override bool Equals(object obj)
        {
            return Equals(obj as CustomStrategy);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Decider != null ? Decider.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ MaxNumberOfRetries.GetHashCode();
                hashCode = (hashCode * 397) ^ WithinTimeRangeMilliseconds.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }


    /// <summary>
    /// TBD
    /// </summary>
    public class CustomDecider : IDecider
    {
        private readonly Func<Exception, Directive> _decider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDecider"/> class.
        /// </summary>
        /// <param name="decider">TBD</param>
        public CustomDecider(Func<Exception, Directive> decider)
        {
            _decider = decider;
        }

        /// <summary>
        /// Determines which <see cref="Directive"/> to use for the specified <paramref name="cause"/>.
        /// </summary>
        /// <param name="cause">The exception that is being mapped.</param>
        /// <returns>The directive used when the given exception is encountered.</returns>
        public Directive Decide(Exception cause)
        {
            return _decider(cause);
        }


    }
}
