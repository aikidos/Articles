# Строгая типизация данных

1. [Введение](#введение)
1. [Коды](#коды)
    * [Проблема](#проблема) 
    * [Решение](#решение)
1. [Денежные значения](#денежные-значения)
    * [Проблема](#проблема-1) 
    * [Решение](#решение)
    * [Тип валюты](#тип-валюты)
1. [Заключение](#заключение)

### Введение

Типизировать можно практически все, но в данной статье мы рассмотрим типизацию только кодов и денежных значений.
Примеры описаны на языке программирования C#, с применением универсальных шаблонов ([generics](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/)). 
Цель статьи - не показать, что только в проектах, написанных на C#, необходима строгая типизация действительно важных значений. 
Это применимо и к другим языкам программирования. Может отличаться лишь реализация, в силу тех или иных особенностей конкретного языка ([units of measure](https://fsharpforfunandprofit.com/posts/units-of-measure/), [union types](https://dotty.epfl.ch/docs/reference/new-types/union-types.html), [newtype](https://wiki.haskell.org/Newtype) и т.д.).

### Коды
#### Проблема

Рассмотрим небольшой участок кода:

```c#
long cityId = CreateCity(new City { Name = "Moscow" });
long groupId = CreateUserGroup(new UserGroup { Name = "Administrator"} );

var user = new User
{
    CityId = cityId,
    GroupId = cityId, // <- Ctrl+C, Ctrl+V
    Name = "Finn",
    ...
};

long userId = CreateUser(user);
```

Этот код скомпилируется, а к чему приведёт его выполнение - неизвестно. В самом лучшем случае, будет исключение на этапе выполнения.
Такие ошибки достаточно сложно выявить и всё усложняется ещё и тем, что в реальных приложениях у пользователя может быть не один десяток полей с кодами.

#### Решение

Добавим тип `IdOf<T>`:

```c#
public readonly struct IdOf<T>
{
    public long Id { get; }
    
    public IdOf(long id) => Id = id;
}
```

Внесем правки с учетом нашего нового типа:

```c#
IdOf<City> CreateCity(City city) { ... }
IdOf<UserGroup> CreateUserGroup(UserGroup userGroup) { ... }
IdOf<User> CreateUser(User user) { ... }

...

 public sealed class User
 {
     ...

     public IdOf<City> CityId { get; set; }

     public IdOf<UserGroup> GroupId { get; set; }

     ...
 }

```

Теперь участок кода, приведенный в качестве проблемного, просто не скомпилируется:

```c#
IdOf<City> cityId = CreateCity(new City { Name = "Moscow" });
IdOf<UserGroup> groupId = CreateUserGroup(new UserGroup { Name = "Administrator" });

var user = new User
{
    CityId = cityId,
    GroupId = cityId, // Cannot implicitly convert type 'IdOf<City>' to 'IdOf<UserGroup>'
    Name = "Finn"
};

IdOf<User> userId = CreateUser(user);
```

### Денежные значения
#### Проблема

Для хранения денежных значений мы зачастую используем `decimal` или `integer`/`long` (зависит от того, используете ли вы [целочисленное программирование](https://en.wikipedia.org/wiki/Integer_programming)).
Однако, все эти типы достаточно общие, что несёт определенные риски, схожие с рисками, описанными для [кодов](#проблема).

#### Решение

Добавим новый тип `Money`:

```c#
public readonly struct Money : IComparable<Money>
{
    public decimal Amount { get; }

    public Money(decimal amount) => Amount = amount;

    public int CompareTo(Money other) => Amount.CompareTo(other.Amount);

    public bool Equals(Money other) => Amount == other.Amount;

    public override bool Equals(object obj) => obj is Money other && Equals(other);

    public static Money operator +(Money a, Money b) => new Money(a.Amount + b.Amount);

    public static Money operator -(Money a, Money b) => new Money(a.Amount - b.Amount);

    public static bool operator >(Money a, Money b) => a.Amount > b.Amount;
    ...
}
```

Все перегрузки операторов перечислять нет смысла. Этот список может быть достаточно объемным и отличаться от проекта к проекту.

#### Тип валюты

Можно справедливо заметить, что есть же ещё и разные типы валют.  
Есть несколько способов реализовать проверку валюты, но мы снова воспользуемся универсальными шаблонами.

Для начала, в качестве примера, выделим два типа валюты, рубли и евро:

```c#
public interface ICurrency
{ }

public readonly struct Rub : ICurrency
{ }

public readonly struct Euro : ICurrency
{ }
```

Внесем правки в описание нашего типа `Money`:

```c#
public readonly struct Money<TCurrency> : IComparable<Money<TCurrency>>
    where TCurrency : struct, ICurrency
{
    ...
}
```

```c#
var balance = new Money<Rub>(100);
```

В самом типе валюты можно хранить дополнительную информацию. Воспользуемся такой возможностью и добавим коды валют:

```c#
public interface ICurrency
{
    string Code { get; }

    int Number { get; }
}

public readonly struct Rub : ICurrency
{
    public string Code => "RUB";

    public int Number => 643;
}

public readonly struct Euro : ICurrency
{
    public string Code => "EUR";

    public int Number => 978;
}
```

Небольшой хак:

```c#
public readonly struct Money<TCurrency> : IComparable<Money<TCurrency>>
    where TCurrency : struct, ICurrency
{
    ...

    public (string Code, int Number) GetCurrencyInfo()
    {
        var currency = default(TCurrency);
    
        return (currency.Code, currency.Number);
    }

    ...
}
```

```c#
var balance = new Money<Rub>(100);

var (code, number) = balance.GetCurrencyInfo();

Console.WriteLine(code);   // Rub
Console.WriteLine(number); // 643
```

Используя данный подход, удобно реализовывать различные сервисы конвертации валют:

```c#
public interface ICurrencyConverterService
{
    Money<TDestination> Convert<TSource, TDestination>(Money<TSource> money)
        where TSource : struct, ICurrency 
        where TDestination : struct, ICurrency;
}

...

ICurrencyConverterService service = ...;

Money<Euro> euro = service.Convert<Rub, Euro>(new Money<Rub>(100));
```

### Заключение

Важно понимать, что в вашем проекте действительно важно и где цена ошибки слишком высока. Эту самую часть проекта можно обезопасить типами, чтобы получился код с проверками на этапе компиляции.

Спасибо за внимание.
