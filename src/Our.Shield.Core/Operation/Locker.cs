using System;
using System.Threading;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Our.Shield.Core.Operation
{
	public class Locker
	{
#if TRACE
        private const int LockWait = 1000000;
#else
        private const int LockWait = 1000;
#endif
		private readonly ReaderWriterLockSlim _slim = new ReaderWriterLockSlim();

		public bool Read(Action execute
#if TRACE
			,[CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0
#endif
			)

		{
#if TRACE
			var callingMethodDebug = sourceFilePath + ":" + sourceLineNumber + " Locker.Read() = ";
#endif
			var hasLocked = false;
			try
			{
				if (!_slim.TryEnterReadLock(LockWait))
				{
#if TRACE
					Debug.WriteLine(callingMethodDebug + "failed to require lock");
#endif
					return false;
				}
#if TRACE
				Debug.WriteLine(callingMethodDebug + "required lock");
#endif
				hasLocked = true;
				execute();
				return true;
			}
			finally
			{
				if (hasLocked)
				{
#if TRACE
					Debug.WriteLine(callingMethodDebug + "released lock");
#endif
					_slim.ExitReadLock();
				}
			}
		}

		public T Read<T>(Func<T> execute
#if TRACE
		, [CallerMemberName] string memberName = "", 
			[CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0
#endif			
			)
		{
#if TRACE
			var callingMethodDebug = sourceFilePath + ":" + sourceLineNumber + " Locker.Read<T>() = ";
#endif
			var hasLocked = false;
			try
			{
				if (!_slim.TryEnterReadLock(LockWait))
				{
#if TRACE
					Debug.WriteLine(callingMethodDebug + "failed to require lock");
#endif
					return default(T);
				}
#if TRACE
				Debug.WriteLine(callingMethodDebug + "required lock");
#endif
				hasLocked = true;
				return execute();
			}
			finally
			{
				if (hasLocked)
				{
#if TRACE
					Debug.WriteLine(callingMethodDebug + "released lock");
#endif
					_slim.ExitReadLock();
				}
			}
		}

		public bool Write(Action execute
#if TRACE
		, [CallerMemberName] string memberName = "", 
			[CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0
#endif
			)
		{
#if TRACE
			var callingMethodDebug = sourceFilePath + ":" + sourceLineNumber + " Locker.Write() = ";
#endif
			var hasLocked = false;
			try
			{
				if (!_slim.TryEnterWriteLock(LockWait))
				{
#if TRACE
					Debug.WriteLine(callingMethodDebug + "failed to require lock");
#endif
					return false;
				}
#if TRACE
				Debug.WriteLine(callingMethodDebug + "required lock");
#endif
				hasLocked = true;
				execute();
				return true;
			}
			finally
			{
				if (hasLocked)
				{
#if TRACE
					Debug.WriteLine(callingMethodDebug + "released lock");
#endif
					_slim.ExitWriteLock();
				}
			}
		}

		public T Write<T>(Func<T> execute
#if TRACE
		, [CallerMemberName] string memberName = "", 
			[CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0
#endif			
			)
		{
#if TRACE
			var callingMethodDebug = sourceFilePath + ":" + sourceLineNumber + " Locker.Write<T>() = ";
#endif
			var hasLocked = false;
			try
			{
				if (!_slim.TryEnterWriteLock(LockWait))
				{
#if TRACE
					Debug.WriteLine(callingMethodDebug + "failed to require lock");
#endif
					return default(T);
				}
#if TRACE
				Debug.WriteLine(callingMethodDebug + "required lock");
#endif
				hasLocked = true;
				return execute();
			}
			finally
			{
				if (hasLocked)
				{
#if TRACE
					Debug.WriteLine(callingMethodDebug + "released lock");
#endif
					_slim.ExitWriteLock();
				}
			}
		}

		bool Upgradable(Func<bool> executeReadBefore, Action executeWrite, Action<bool> executeReadAfter)
		{
			var hasReadLocked = false;
			try
			{
				if (!_slim.TryEnterUpgradeableReadLock(LockWait))
				{
					return false;
				}
				hasReadLocked = true;
				var doRunWrite = executeReadBefore();
				if (doRunWrite)
				{
					var hasWriteLock = false;
					try
					{
						if (!_slim.TryEnterWriteLock(LockWait))
						{
							return false;
						}
						hasWriteLock = true;
						executeWrite();
					}
					finally
					{
						if (hasWriteLock)
						{
							_slim.ExitWriteLock();
						}
					}
				}
                executeReadAfter?.Invoke(doRunWrite);
                return true;
			}
			finally
			{
				if (hasReadLocked)
				{
					_slim.ExitUpgradeableReadLock();
				}
			}
		}
	}
}
