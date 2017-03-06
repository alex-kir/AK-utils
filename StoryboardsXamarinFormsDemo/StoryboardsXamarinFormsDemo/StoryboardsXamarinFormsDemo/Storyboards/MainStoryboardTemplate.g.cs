// ------------------ auto-generated file ---------------------- //
// using StoryboardsXamarinFormsDemo
// 
// namespace StoryboardsXamarinFormsDemo.Storyboards

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StoryboardsXamarinFormsDemo;

namespace StoryboardsXamarinFormsDemo.Storyboards
{
    internal abstract class MainStoryboardTemplate
    {
        private Func<CancellationToken, Task> stateRunner;
        protected CancellationTokenSource cts;
		
        private static CancellationTokenSource NewCts(ref CancellationTokenSource cts, params CancellationToken[] tokens)
        {
            var newCts = tokens.Length == 0 ? new CancellationTokenSource() : CancellationTokenSource.CreateLinkedTokenSource(tokens);
            var tmp = Interlocked.Exchange(ref cts, newCts);
            if (tmp != null)
                tmp.Cancel();
            return newCts;
        }

        private static void DeleteCts(ref CancellationTokenSource cts)
        {
            var tmp = Interlocked.Exchange(ref cts, null);
            if (tmp != null)
                tmp.Cancel();
        }

        public async Task RunFromStartOne(string arg0, CancellationToken externalToken)
        {
		    System.Diagnostics.Debug.WriteLine("MainStoryboardTemplate: started from StartOne(string)");
			var token = NewCts(ref cts, externalToken).Token;
			Continue(MyPageOneStateRunner, new MyPageOneState(), arg0);
			await Run(token);
        }
        public async Task RunFromStartTwo(CancellationToken externalToken)
        {
		    System.Diagnostics.Debug.WriteLine("MainStoryboardTemplate: started from StartTwo()");
			var token = NewCts(ref cts, externalToken).Token;
			Continue(MyPageTwoStateRunner, new MyPageTwoState());
			await Run(token);
        }

		private async Task Run(CancellationToken token)
        {
			try
			{
				while (!token.IsCancellationRequested)
					await stateRunner(token);
			}
			catch (OperationCanceledException)
			{
				System.Diagnostics.Debug.WriteLine("MainStoryboardTemplate: stopped");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("MainStoryboardTemplate: stopped by " + ex.Message);
				throw;
			}
        }

        protected void Stop()
        {
            DeleteCts(ref cts);
        }

        protected void Continue<TState>(Func<CancellationToken, TState, Task> nextRunner, TState state)
        {
            stateRunner = token => nextRunner(token, state);
        }

		protected void Continue<TState, T1>(Func<CancellationToken, TState, T1, Task> nextRunner, TState state, T1 arg1)
        {
            stateRunner = token => nextRunner(token, state, arg1);
        }

		protected void Continue<TState, T1, T2>(Func<CancellationToken, TState, T1, T2, Task> nextRunner, TState state, T1 arg1, T2 arg2)
        {
            stateRunner = token => nextRunner(token, state, arg1, arg2);
        }

		protected void Continue<TState, T1, T2, T3>(Func<CancellationToken, TState, T1, T2, T3, Task> nextRunner, TState state, T1 arg1, T2 arg2, T3 arg3)
        {
            stateRunner = token => nextRunner(token, state, arg1, arg2, arg3);
        }

		// ------------------ transitions --------------------------- //

		protected abstract void MyPageOne(MyPageOneState state, string arg0);
		
		async Task MyPageOneStateRunner(CancellationToken token, MyPageOneState state, string arg0)
        {
			System.Diagnostics.Debug.WriteLine("STATE(MainStoryboardTemplate): MyPageOne");
			MyPageOne(state, arg0);

            (await state.GetResult(token)).Match(
				@showtwo: (it0, it1) => Continue(MyPageTwoStateRunner, new MyPageTwoState(), it0, it1)
                );
        }

		protected abstract void MyPageThree(MyPageThreeState state);
		
		async Task MyPageThreeStateRunner(CancellationToken token, MyPageThreeState state)
        {
			System.Diagnostics.Debug.WriteLine("STATE(MainStoryboardTemplate): MyPageThree");
			MyPageThree(state);

            (await state.GetResult(token)).Match(
				@back: () => Continue(MyPageTwoStateRunner, new MyPageTwoState())
                );
        }

		protected abstract void MyPageTwo(MyPageTwoState state);
		
		async Task MyPageTwoStateRunner(CancellationToken token, MyPageTwoState state)
        {
			System.Diagnostics.Debug.WriteLine("STATE(MainStoryboardTemplate): MyPageTwo");
			MyPageTwo(state);

            (await state.GetResult(token)).Match(
				@showone: (it0) => Continue(MyPageOneStateRunner, new MyPageOneState(), it0)
				, @showthree: () => Continue(MyPageThreeStateRunner, new MyPageThreeState())
                );
        }

		protected abstract void MyPageTwo(MyPageTwoState state, string arg0, int arg1);
		
		async Task MyPageTwoStateRunner(CancellationToken token, MyPageTwoState state, string arg0, int arg1)
        {
			System.Diagnostics.Debug.WriteLine("STATE(MainStoryboardTemplate): MyPageTwo");
			MyPageTwo(state, arg0, arg1);

            (await state.GetResult(token)).Match(
				@showone: (it0) => Continue(MyPageOneStateRunner, new MyPageOneState(), it0)
				, @showthree: () => Continue(MyPageThreeStateRunner, new MyPageThreeState())
                );
        }

		// ------------------ states --------------------------- //

		internal class State<T>
		{
			protected TaskCompletionSource<T> completion = new TaskCompletionSource<T>();

			public async Task<T> GetResult(CancellationToken token)
			{
				using (var registration = token.Register(() => completion.TrySetCanceled()))
				{
					return await completion.Task;
				}
			}

			public Task<T> GetResult()
			{
				return completion.Task;
			}
		}

		internal partial class MyPageOneState : State<MyPageOneState.Result>
		{
			internal abstract class Result
			{
				public abstract void Match(Action<string, int> @showtwo);

				internal class ShowTwo : Result
				{
					private string result0;
					private int result1;
					public ShowTwo(string arg0, int arg1)
					{
					this.result0 = arg0;
					this.result1 = arg1;
					}

					public override void Match(Action<string, int> @showtwo)
					{
						System.Diagnostics.Debug.WriteLine("STATE(MainStoryboardTemplate): +---> showtwo");
						@showtwo(result0, result1);
					}
				}
			}

			public void CompleteWithShowTwo(string arg0, int arg1)
			{
				completion.TrySetResult(new Result.ShowTwo(arg0, arg1));
			}
		}
		internal partial class MyPageThreeState : State<MyPageThreeState.Result>
		{
			internal abstract class Result
			{
				public abstract void Match(Action @back);

				internal class Back : Result
				{
					public Back()
					{
					}

					public override void Match(Action @back)
					{
						System.Diagnostics.Debug.WriteLine("STATE(MainStoryboardTemplate): +---> back");
						@back();
					}
				}
			}

			public void CompleteWithBack()
			{
				completion.TrySetResult(new Result.Back());
			}
		}
		internal partial class MyPageTwoState : State<MyPageTwoState.Result>
		{
			internal abstract class Result
			{
				public abstract void Match(Action<string> @showone, Action @showthree);

				internal class ShowOne : Result
				{
					private string result0;
					public ShowOne(string arg0)
					{
					this.result0 = arg0;
					}

					public override void Match(Action<string> @showone, Action @showthree)
					{
						System.Diagnostics.Debug.WriteLine("STATE(MainStoryboardTemplate): +---> showone");
						@showone(result0);
					}
				}

				internal class ShowThree : Result
				{
					public ShowThree()
					{
					}

					public override void Match(Action<string> @showone, Action @showthree)
					{
						System.Diagnostics.Debug.WriteLine("STATE(MainStoryboardTemplate): +---> showthree");
						@showthree();
					}
				}
			}

			public void CompleteWithShowOne(string arg0)
			{
				completion.TrySetResult(new Result.ShowOne(arg0));
			}
			public void CompleteWithShowThree()
			{
				completion.TrySetResult(new Result.ShowThree());
			}
		}

    }
}

