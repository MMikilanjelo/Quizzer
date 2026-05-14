using Source.Features.Quizzes.ActiveQuiz.Models;
using Source.Features.Quizzes.ActiveQuiz.UseCases;
using Source.Features.Quizzes.CreateQuiz.Models;
using Source.Features.Quizzes.CreateQuiz.UseCases;
using Source.Features.Quizzes.Factory;
using Source.Features.Quizzes.FinishedQuiz.UseCases;
using Source.Features.Quizzes.Mediator;
using Source.Features.Quizzes.MyQuizzes.UseCases;
using Source.Shared;
using VContainer;

namespace Source.Features.Quizzes
{
    public static class DependencyInjection
    {
        public static IContainerBuilder RegisterQuizzesFeature(this IContainerBuilder builder)
        {
            builder.Register<IQuizzesUIFactory, QuizzesUIFactory>(Lifetime.Singleton);
            builder.Register<IQuizzesMediator, QuizzesMediator>(Lifetime.Singleton);

            builder.Register<IQuizCreationRepository, QuizCreationRepository>(Lifetime.Singleton);
            builder.Register<IUseCase<FetchQuizConfiguration.Response>, FetchQuizConfiguration.UseCase>(Lifetime.Singleton);
            builder.Register<IUseCase<SmartScheduleQuiz.Response>, SmartScheduleQuiz.UseCase>(Lifetime.Singleton);
            builder.Register<IUseCase<ManualScheduleQuiz.Response>, ManualScheduleQuiz.UseCase>(Lifetime.Singleton);
            builder.Register<IUseCase<FetchQuizzes.Request, FetchQuizzes.Response>, FetchQuizzes.UseCase>(Lifetime.Singleton);
            builder.Register<IUseCase<FetchQuiz.Request, FetchQuiz.Response>, FetchQuiz.UseCase>(Lifetime.Singleton);
            builder.Register<IUseCase<SubmitAnswer.Request, SubmitAnswer.Response>, SubmitAnswer.UseCase>(Lifetime.Singleton);
            builder.Register<IUseCase<FetchQuizAnalytics.Request, FetchQuizAnalytics.Response>, FetchQuizAnalytics.UseCase>(Lifetime.Singleton);

            builder.Register<IActiveQuizRepository, ActiveQuizRepository>(Lifetime.Singleton);

            return builder;
        }
    }
}