using System;
using System.Linq;
using System.Threading.Tasks;
using AbpSolution1.Authors;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace AbpSolution1.Books;

public abstract class BookAppService_Tests<TStartupModule> : AbpSolution1ApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IBookAppService _bookAppService;
    private readonly IRepository<Author, Guid> _authorRepository;

    protected BookAppService_Tests()
    {
        _bookAppService = GetRequiredService<IBookAppService>();
        _authorRepository = GetRequiredService<IRepository<Author, Guid>>();
    }

    [Fact]
    public async Task Should_Get_List_Of_Books()
    {
        //Act
        var result = await _bookAppService.GetListAsync(
            new PagedAndSortedResultRequestDto()
        );

        //Assert
        result.TotalCount.ShouldBeGreaterThan(0);
        result.Items.ShouldContain(b => b.Name == "1984");
    }

    [Fact]
    public async Task Should_Create_A_Valid_Book()
    {
        var authorId = await GetExistingAuthorIdAsync();

        //Act
        var result = await _bookAppService.CreateAsync(
            new CreateUpdateBookDto
            {
                Name = "New test book 42",
                AuthorId = authorId,
                Price = 10,
                PublishDate = DateTime.Now,
                Type = BookType.ScienceFiction
            }
        );

        //Assert
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("New test book 42");
    }
    
    [Fact]
    public async Task Should_Not_Create_A_Book_Without_Name()
    {
        var authorId = await GetExistingAuthorIdAsync();

        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _bookAppService.CreateAsync(
                new CreateUpdateBookDto
                {
                    Name = "",
                    AuthorId = authorId,
                    Price = 10,
                    PublishDate = DateTime.Now,
                    Type = BookType.ScienceFiction
                }
            );
        });

        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "Name"));
    }

    private async Task<Guid> GetExistingAuthorIdAsync()
    {
        var author = await _authorRepository.FindAsync(author => author.Name == "George Orwell");
        author.ShouldNotBeNull();
        return author.Id;
    }
}
