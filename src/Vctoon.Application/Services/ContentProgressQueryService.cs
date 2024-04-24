using Vctoon.Comics.Dtos;
using Vctoon.Libraries;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Vctoon.Services;

public class ContentProgressQueryService(IContentProgressRepository contentProgressRepository)
    : VctoonService, ITransientDependency
{
    public async Task AppendCompletionRateAsync(Guid userId, List<ComicChapterDto> chapters)
    {
        Check.NotNullOrEmpty(chapters, nameof(chapters));

        var processes =
            await contentProgressRepository.GetListAsync(userId, comicChapterIds: chapters.Select(x => x.Id));
        foreach (var chapter in chapters)
        {
            chapter.Progress = processes.FirstOrDefault(x => x.ComicChapterId == chapter.Id)?.CompletionRate ?? 0;
        }
    }

    public async Task AppendCompletionRateAsync(Guid userId, ComicChapterDto chapter)
    {
        var process = await contentProgressRepository.GetOrDefaultAsync(userId, comicChapterId: chapter.Id);
        if (process != null)
        {
            chapter.Progress = process.CompletionRate;
        }
    }

    public async Task AppendCompletionRateAsync(Guid userId, List<ComicDto> comics)
    {
        Check.NotNullOrEmpty(comics, nameof(comics));

        var processes = await contentProgressRepository.GetListAsync(userId, comics.Select(x => x.Id));
        foreach (var comic in comics)
        {
            comic.Progress = processes.FirstOrDefault(x => x.ComicId == comic.Id)?.CompletionRate ?? 0;
        }
    }

    public async Task AppendCompletionRateAsync(Guid userId, ComicDto comic)
    {
        var process = await contentProgressRepository.GetOrDefaultAsync(userId, comic.Id);
        if (process != null)
        {
            comic.Progress = process.CompletionRate;
        }
    }
}