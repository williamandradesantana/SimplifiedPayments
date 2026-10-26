using FluentAssertions;
using SimplifiedPayments.Application.Exceptions;
using SimplifiedPayments.Application.UseCases.Users;
using SimplifiedPayments.Domain.Enums;
using SimplifiedPayments.Domain.Exceptions;
using SimplifiedPayments.Tests.Fakes;
using SimplifiedPayments.Tests.Helpers;

namespace SimplifiedPayments.Tests.UseCases;

public class CreateUserHandlerTests
{
    private readonly FakeUserRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
        => _handler = new CreateUserHandler(
            _repository, _unitOfWork, new FakePasswordHasher()
        );

    private static CreateUserCommand ValidCommand(
        string? document = null, string email = "maria@example.com",
        string password = "password", UserType type = UserType.Common
    ) => new("Maria Silva", document ?? UserFactory.ValidCpf, email, password, type);

    [Fact]
    public async Task Handle_ValidCommand_PersistUserWithHashedPassword()
    {
        CreateUserResult result = await _handler.HandleAsync(
            ValidCommand(document: "529.982.247-25", email: "Maria@Example.com")
        );
        result.Document.Should().Be("52998224725");
        result.Email.Should().Be("maria@example.com");

        var saved = _repository.Users.Should().ContainSingle().Subject;
        saved.PasswordHash.Should().Be("hashed:password");
        saved.PasswordHash.Should().NotBe("password");
        _unitOfWork.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_DuplicateDocument_ThrowsConflictAndDoesNotSave()
    {
        await _handler.HandleAsync(ValidCommand());

        Func<Task> act = () => _handler.HandleAsync(ValidCommand(document: "529.982.247-25"));

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Document already registered!");
        _repository.Users.Should().HaveCount(1);
        _unitOfWork.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_DuplicateEmailIgnoringCase_ThrowsConflict()
    {
        await _handler.HandleAsync(ValidCommand());

        Func<Task> act = () => _handler.HandleAsync(
            ValidCommand(document: "29141777638", email: "MARIA@example.com")
        );

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Email already registered!");
        _repository.Users.Should().HaveCount(1);
        _unitOfWork.SaveCount.Should().Be(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234567")]
    public async Task Handle_WeakPassword_ThrowsInvalidRequest(string password)
    {
        Func<Task> act = () => _handler.HandleAsync(ValidCommand(password: password));
        await act.Should().ThrowAsync<InvalidRequestException>();
        _repository.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PasswordOver72Bytes_ThrowsInvalidRequest()
    {
        Func<Task> act = () => _handler.HandleAsync(ValidCommand(password: new string('a', 73)));
        await act.Should().ThrowAsync<InvalidRequestException>();
        _repository.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_InvalidDocument_ThrowsDomainExceptionAndDoesNotSave()
    {
        Func<Task> act = () => _handler.HandleAsync(ValidCommand(document: "11111111111"));
        
        await act.Should().ThrowAsync<InvalidDocumentException>().WithMessage("Invalid CPF.");
        _repository.Users.Should().BeEmpty();
        _unitOfWork.SaveCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShopkeeperWithCnpj_Succeeds()
    {
        CreateUserResult result = await _handler.HandleAsync(
            ValidCommand(
                document: UserFactory.ValidCnpj, 
                email: "loja@email.com", 
                type: UserType.Shopkeeper
            )
        );

        result.Type.Should().Be(UserType.Shopkeeper);
        result.Document.Should().Be(UserFactory.ValidCnpj);
    }
}
