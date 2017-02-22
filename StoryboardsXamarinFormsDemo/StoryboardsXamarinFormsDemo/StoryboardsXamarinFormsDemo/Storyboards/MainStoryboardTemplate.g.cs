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

        public async Task RunFromStartOne(string arg, CancellationToken externalToken)
        {
		    System.Diagnostics.Debug.WriteLine("MainStoryboardTemplate: started from StartOne with string");
			var token = NewCts(ref cts, externalToken).Token;
			Continue(MyPageOneStateRunner, new MyPageOneState(), arg);
			await Run(token);
        }

        public async Task RunFromStartTwo(CancellationToken externalToken)
        {
		    System.Diagnostics.Debug.WriteLine("MainStoryboardTemplate: started from StartTwo");
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

		protected void Continue<TState,T1>(Func<CancellationToken, TState, T1, Task> nextRunner, TState state, T1 arg1)
        {
            stateRunner = token => nextRunner(token, state, arg1);
        }

		// ------------------ transitions --------------------------- //

		protected abstract void MyPageOne(MyPageOneState state, string arg1);
		
		async Task MyPageOneStateRunner(CancellationToken token, MyPageOneState state, string arg1)
        {
			System.Diagnostics.Debug.WriteLine("STATE(MainStoryboardTemplate): MyPageOne");
			MyPageOne(state, arg1);

            (await state.GetResult(token)).Match(
				@showtwo: it => Continue(MyPageTwoStateRunner, new MyPageTwoState(), it)
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
				@showone: it => Continue(MyPageOneStateRunner, new MyPageOneState(), it)
				, @showthree: () => Continue(MyPageThreeStateRunner, new MyPageThreeState())
                );
        }

		protected abstract void MyPageTwo(MyPageTwoState state, string arg1);
		
		async Task MyPageTwoStateRunner(CancellationToken token, MyPageTwoState state, string arg1)
        {
			System.Diagnostics.Debug.WriteLine("STATE(MainStoryboardTemplate): MyPageTwo");
			MyPageTwo(state, arg1);

            (await state.GetResult(token)).Match(
				@showone: it => Continue(MyPageOneStateRunner, new MyPageOneState(), it)
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
				public abstract void Match(Action<string> @showtwo);

				internal class ShowTwo : Result
				{
					private string result;

					public ShowTwo(string result)
					{
						this.result = result;
					}

					public override void Match(Action<string> @showtwo)
					{
						System.Diagnostics.Debug.WriteLine("STATE(MainStoryboardTemplate): +---> showtwo");
						@showtwo(result);
					}
				}
			}


			public void CompleteWithShowTwo(string result)
			{
				completion.TrySetResult(new Result.ShowTwo(result));
			}
		}

		internal partial class MyPageThreeState : State<MyPageThreeState.Result>
		{
			internal abstract class Result
			{
				public abstract void Match(Action @back);

				internal class Back : Result
				{
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
					private string result;

					public ShowOne(string result)
					{
						this.result = result;
					}

					public override void Match(Action<string> @showone, Action @showthree)
					{
						System.Diagnostics.Debug.WriteLine("STATE(MainStoryboardTemplate): +---> showone");
						@showone(result);
					}
				}

				internal class ShowThree : Result
				{
					public override void Match(Action<string> @showone, Action @showthree)
					{
						System.Diagnostics.Debug.WriteLine("STATE(MainStoryboardTemplate): +---> showthree");
						@showthree();
					}
				}
			}


			public void CompleteWithShowOne(string result)
			{
				completion.TrySetResult(new Result.ShowOne(result));
			}

			public void CompleteWithShowThree()
			{
				completion.TrySetResult(new Result.ShowThree());
			}
		}

    }
}

