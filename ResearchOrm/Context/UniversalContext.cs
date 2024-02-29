using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace ResearchOrm.Context;

// TODO: Необходимо доделать функционал. Предусмотреть интерпретатор для операций игнорирования, изменения, получения сущностей и их последовательностей и так же свойств сущностей, а так же установки отношений между сущностями. 
// Вынести в отдельную библиотеку. На будущее - рассмотреть создание своей мини ORM для динамического управления БД и серверами.
// Необходимо внести функциональность изменения и добавленя таблиц в уже существующий контекст без его пересоздания. Пока это сделать не получилось, т.к. модель устанавливается как ReadOnly после выполенения OnModelCreating.
// Изучить возможность вызова OnModelCreating (можно ли обойтись без OnModelCreating и OnConfiguring???) уже в созданном контексте и переопределение схемы БД и зарегистрированных типов.
// Иначе придется каждый раз создавать новый объект контекста, что не будет влиять на производительность, но менее удобно.
// В планах создать инструмент, котрый позволит создавать, редактировать, удалять, менять схему БД и таблиц динамически без применения миграций (либо миграции должны запускаться в рантайме)
// Так же надо првести код в порядок.

/// <summary>
/// Унивесальный контекст данных основанный на EF core 6. На текущий момент умеет создавать БД и таблицы на основе переданных типов.
/// Action<ModelBuilder> _creating позволяет настраивать ModelBilder до создания контекста. 
/// </summary>
public class UniversalContext : DbContext
{
    private IEnumerable<Type> _types = Enumerable.Empty<Type>();
    private IEnumerable<Type> _ignorableTypes = Enumerable.Empty<Type>();
    private Action<ModelBuilder> _creating;
    private Action<ModelBuilder, IEnumerable<Type>> _creatingWithTypes;

    public UniversalContext(DbContextOptions<UniversalContext> opt, IEnumerable<Type> types) : base(opt)
    {
        _types = types;
    }

    public UniversalContext(IEnumerable<Type> types, IEnumerable<Type> ignorableTypes)
    {
        _types = types;
        _ignorableTypes = ignorableTypes;
    }

    public UniversalContext(IEnumerable<Type> types)
    {
        _types = types;
    }

    public UniversalContext(IEnumerable<Type> types, Action<ModelBuilder> creating)
    {
        _creating = creating;
        _types = types;
    }

    public UniversalContext()
    {
    }

    /// <summary>
    /// Sets new connection string
    /// </summary>
    /// <param name="connectionString"></param>
    public void SetConnectionString(string connectionString)
    {
        try
        {
            Database.SetConnectionString(connectionString);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public void SetDbSet<T>() where T : class
    {
        Set<T>();
    }


    /// <summary>
    /// Creates a migration.
    /// </summary>
    public async Task MigrateAsync(CancellationToken cancellation = default)
        => await Database.MigrateAsync(cancellation);

    /// <summary>
    /// Returns true if DB is already existed.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CreateDbAsync(CancellationToken cancellation = default)
    {
        try
        {
            return await Database.EnsureCreatedAsync(cancellation);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    /// Gets the current database model
    /// </summary>
    /// <returns></returns>
    public IModel GetDbModel() => Database.GetService<IDesignTimeModel>().Model;

    public async Task CreateTablesAsync(IEnumerable<Type> types, CancellationToken cancellation = default)
    {
        _types = types;
        var modelBuilder = new ModelBuilder();

        foreach (var type in types)
        {
            try
            {
                modelBuilder.Model.AddEntityType(type);
            }
            catch
            {
                continue;
            }
        }

        OnModelCreating(modelBuilder);

        await CreateTablesAsync(cancellation);
    }

    public async Task CreateTablesAsync(string connectionString, CancellationToken cancellation = default)
    {
        SetConnectionString(connectionString);

        await CreateTablesAsync(cancellation);
    }

    /// <summary>
    /// Creates tables in the database
    /// </summary>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task CreateTablesAsync(CancellationToken cancellation = default)
    {
        try
        {
            await Database.MigrateAsync(cancellation);

            var creatorDependencies = Database.GetService<RelationalDatabaseCreatorDependencies>();
            var model = GetDbModel();
            var commands = creatorDependencies.MigrationsSqlGenerator.Generate(
                creatorDependencies.ModelDiffer.GetDifferences(null, model.GetRelationalModel()),
                model);


            await creatorDependencies.MigrationCommandExecutor.ExecuteNonQueryAsync(commands,
            creatorDependencies.Connection,
            cancellation);
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.Message);
            throw;
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql();

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (_creating != null)
        {
            _creating(modelBuilder);
        }

        if (!_types.Any() || _types == null)
        {
            return;
        }

        if (_ignorableTypes.Any())
        {
            foreach (var type in _ignorableTypes)
            {
                modelBuilder.Ignore(type);
            }
        }

        foreach (var type in _types)
        {
            modelBuilder.Entity(type);
        }

        base.OnModelCreating(modelBuilder);
    }
}