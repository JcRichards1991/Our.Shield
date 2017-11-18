using System;
using System.Threading;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Our.Shield.Core.Operation
{
	public class Locker
	{
#if DEBUG
        private const int LockWait = 1000000;
#else
        private const int LockWait = 1000;
#endif
		private readonly ReaderWriterLockSlim _slim = new ReaderWriterLockSlim();

		public bool Read(Action execute
#if DEBUG
			,[CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0
#endif
			)

		{
#if DEBUG
			var callingMethodDebug = sourceFilePath + ":" + sourceLineNumber + " Locker.Read() = ";
#endif
			var hasLocked = false;
			try
			{
				if (!_slim.TryEnterReadLock(LockWait))
				{
#if DEBUG
					Debug.WriteLine(callingMethodDebug + "failed to require lock");
#endif
					return false;
				}
#if DEBUG
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
#if DEBUG
					Debug.WriteLine(callingMethodDebug + "released lock");
#endif
					_slim.ExitReadLock();
				}
			}
		}

		public T Read<T>(Func<T> execute, [CallerMemberName] string memberName = "", 
			[CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
			var callingMethodDebug = sourceFilePath + ":" + sourceLineNumber + " Locker.Read<T>() = ";
			var hasLocked = false;
			try
			{
				if (!_slim.TryEnterReadLock(LockWait))
				{
					Debug.WriteLine(callingMethodDebug + "failed to require lock");
					return default(T);
				}
				Debug.WriteLine(callingMethodDebug + "required lock");
				hasLocked = true;
				return execute();
			}
			finally
			{
				if (hasLocked)
				{
					Debug.WriteLine(callingMethodDebug + "released lock");
					_slim.ExitReadLock();
				}
			}
		}

		public bool Write(Action execute, [CallerMemberName] string memberName = "", 
			[CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
			var callingMethodDebug = sourceFilePath + ":" + sourceLineNumber + " Locker.Write() = ";
			var hasLocked = false;
			try
			{
				if (!_slim.TryEnterWriteLock(LockWait))
				{
					Debug.WriteLine(callingMethodDebug + "failed to require lock");
					return false;
				}
				Debug.WriteLine(callingMethodDebug + "required lock");
				hasLocked = true;
				execute();
				return true;
			}
			finally
			{
				if (hasLocked)
				{
					Debug.WriteLine(callingMethodDebug + "released lock");
					_slim.ExitWriteLock();
				}
			}
		}

		public T Write<T>(Func<T> execute, [CallerMemberName] string memberName = "", 
			[CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
			var callingMethodDebug = sourceFilePath + ":" + sourceLineNumber + " Locker.Write<T>() = ";
			var hasLocked = false;
			try
			{
				if (!_slim.TryEnterWriteLock(LockWait))
				{
					Debug.WriteLine(callingMethodDebug + "failed to require lock");
					return default(T);
				}
				Debug.WriteLine(callingMethodDebug + "required lock");
				hasLocked = true;
				return execute();
			}
			finally
			{
				if (hasLocked)
				{
					Debug.WriteLine(callingMethodDebug + "released lock");
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
