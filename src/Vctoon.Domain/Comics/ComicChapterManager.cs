using Vctoon.Libraries;

namespace Vctoon.Comics;

public class ComicChapterManager(
    IComicChapterRepository comicChapterRepository,
    IContentProgressRepository contentProgressRepository) : DomainService
{
    public async Task UpdateOrAddProcessAsync(ComicChapter comicChapter, Guid userId, double process)
    {
        var comicChapterContentProgress =
            await contentProgressRepository.GetOrDefaultAsync(userId, comicChapterId: comicChapter.Id);

        if (comicChapterContentProgress == null)
        {
            comicChapterContentProgress = new ContentProgress(
                GuidGenerator.Create(),
                userId,
                process,
                comicChapterId: comicChapter.Id
            );

            await contentProgressRepository.InsertAsync(comicChapterContentProgress);
        }
        else
        {
            comicChapterContentProgress.CompletionRate = process;
        }

        await contentProgressRepository.UpdateAsync(comicChapterContentProgress);


        // Update comic completion rate

        var comicRate = await contentProgressRepository.GetComicCompletionRateAsync(comicChapter.ComicId, userId);

        var comicContentProgress =
            await contentProgressRepository.GetOrDefaultAsync(userId, comicChapter.ComicId);

        if (comicContentProgress == null)
        {
            comicContentProgress = new ContentProgress(
                GuidGenerator.Create(),
                userId,
                comicRate,
                comicChapter.ComicId
            );

            await contentProgressRepository.InsertAsync(comicContentProgress);
        }
        else
        {
            comicContentProgress.CompletionRate = comicRate;
        }

        await contentProgressRepository.UpdateAsync(comicContentProgress);
    }
}