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
		private ReaderWriterLockSlim slim = new ReaderWriterLockSlim();

		public bool Read(Action execute
#if DEBUG
			,[CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0
#endif
			)

		{
#if DEBUG
			var callingMethodDebug = sourceFilePath + ":" + sourceLineNumber.ToString() + " Locker.Read() = ";
#endif
			bool hasLocked = false;
			try
			{
				if (!slim.TryEnterReadLock(LockWait))
				{
#if DEBUG
					System.Diagnostics.Debug.WriteLine(callingMethodDebug + "failed to require lock");
#endif
					return false;
				}
#if DEBUG
				System.Diagnostics.Debug.WriteLine(callingMethodDebug + "required lock");
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
					System.Diagnostics.Debug.WriteLine(callingMethodDebug + "released lock");
#endif
					slim.ExitReadLock();
				}
			}
		}

		public T Read<T>(Func<T> execute, [CallerMemberName] string memberName = "", 
			[CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
			var callingMethodDebug = sourceFilePath + ":" + sourceLineNumber.ToString() + " Locker.Read<T>() = ";
			bool hasLocked = false;
			try
			{
				if (!slim.TryEnterReadLock(LockWait))
				{
					System.Diagnostics.Debug.WriteLine(callingMethodDebug + "failed to require lock");
					return default(T);
				}
				System.Diagnostics.Debug.WriteLine(callingMethodDebug + "required lock");
				hasLocked = true;
				return execute();
			}
			finally
			{
				if (hasLocked)
				{
					System.Diagnostics.Debug.WriteLine(callingMethodDebug + "released lock");
					slim.ExitReadLock();
				}
			}
		}

		public bool Write(Action execute, [CallerMemberName] string memberName = "", 
			[CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
			var callingMethodDebug = sourceFilePath + ":" + sourceLineNumber.ToString() + " Locker.Write() = ";
			bool hasLocked = false;
			try
			{
				if (!slim.TryEnterWriteLock(LockWait))
				{
					System.Diagnostics.Debug.WriteLine(callingMethodDebug + "failed to require lock");
					return false;
				}
				System.Diagnostics.Debug.WriteLine(callingMethodDebug + "required lock");
				hasLocked = true;
				execute();
				return true;
			}
			finally
			{
				if (hasLocked)
				{
					System.Diagnostics.Debug.WriteLine(callingMethodDebug + "released lock");
					slim.ExitWriteLock();
				}
			}
		}

		public T Write<T>(Func<T> execute, [CallerMemberName] string memberName = "", 
			[CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
			var callingMethodDebug = sourceFilePath + ":" + sourceLineNumber.ToString() + " Locker.Write<T>() = ";
			bool hasLocked = false;
			try
			{
				if (!slim.TryEnterWriteLock(LockWait))
				{
					System.Diagnostics.Debug.WriteLine(callingMethodDebug + "failed to require lock");
					return default(T);
				}
				System.Diagnostics.Debug.WriteLine(callingMethodDebug + "required lock");
				hasLocked = true;
				return execute();
			}
			finally
			{
				if (hasLocked)
				{
					System.Diagnostics.Debug.WriteLine(callingMethodDebug + "released lock");
					slim.ExitWriteLock();
				}
			}
		}

		bool Upgradable(Func<bool> executeReadBefore, Action executeWrite, Action<bool> executeReadAfter)
		{
			bool hasReadLocked = false;
			try
			{
				if (!slim.TryEnterUpgradeableReadLock(LockWait))
				{
					return false;
				}
				hasReadLocked = true;
				var doRunWrite = executeReadBefore();
				if (doRunWrite)
				{
					bool hasWriteLock = false;
					try
					{
						if (!slim.TryEnterWriteLock(LockWait))
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
							slim.ExitWriteLock();
						}
					}
				}
				if (executeReadAfter != null)
				{
					executeReadAfter(doRunWrite);
				}
				return true;
			}
			finally
			{
				if (hasReadLocked)
				{
					slim.ExitUpgradeableReadLock();
				}
			}
		}
	}
}
