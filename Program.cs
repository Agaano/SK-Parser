using System.Text;
using System.Text.Json;

//Функция форматирования, каждое поле перед тем как вставить в csv надо привести в порядок
//конкретно здесь убираются все переносы строк, одинарные кавычки заменяются на двойные и по бокам от поля ставятся кавычки
string Format(string? str) {
    return $"\"{str?.Replace("\n", "").Replace("\r", "").Replace("\"", "\"\"")}\"";
}

//Функция старта
async void Start() {
    HttpClient client = new();

    //здесь получаем кол-во компаний
    HttpResponseMessage responseMessage = await client.GetAsync("https://navigator.sk.ru/navigator/api/overall/company_stat");
    ResponseJsonCompanyCountClass? response1 = JsonSerializer.Deserialize<ResponseJsonCompanyCountClass>(await responseMessage.Content.ReadAsStreamAsync());
    
    //Вот здесь создаётся тело для запроса
    HttpContent content = new StringContent(
        JsonSerializer.Serialize(new {
            filter=new {
                tags_type="or"
            },
            limit=response1?.company_count,
            page=1,
            sort="-member_since",
        }), Encoding.UTF8, "application/json"
    );
    using HttpRequestMessage requestMessage = new(HttpMethod.Post, "https://navigator.sk.ru/navigator/api/search/only_companies/");
    requestMessage.Content = content;
    //Добавляются куки файлы
    requestMessage.Headers.Add("Cookie", "navigator_session=eyJfZnJlc2giOmZhbHNlLCJ1c2VyX2lkIjowfQ.Zq1vfA.84AqtPTFijINEGUM2FnoSC51gRg; navigator_session=eyJfZnJlc2giOmZhbHNlLCJ1c2VyX2lkIjowfQ.Zq1m2A.JAi9wQ0xCzujJftPkl0Tip9Oa1c; navigator_session_logs=c789d6021eddb5247f11226f4c007d3c; sk_lang=ru");
    
    //запрос отправляется
    HttpResponseMessage response = await client.SendAsync(requestMessage);
    //преобразуется в строку
    string stream = await response.Content.ReadAsStringAsync();
    
    //строка преобразуется в полноценный объект с которым удобно работать
    ResponseJsonClass? json = JsonSerializer.Deserialize<ResponseJsonClass>(stream);
    Console.WriteLine(stream);
    int id = 1;
    using StreamWriter writer = new("./table.csv");

    //заголовок для csv таблицы
    writer.Write("Номер, ");
    writer.Write("ИНН, ");
    writer.Write("ОРН, ");
    writer.Write("ОГРН, ");
    writer.Write("Юр.лицо, ");
    writer.Write("Название стартапа, ");
    writer.Write("Описание стартапа, ");
    writer.Write("Описание компании, ");
    writer.Write("ФИО Директора, ");
    writer.Write("Сайт, ");
    writer.Write("Отрасль, ");
    writer.Write("Электронная почта 1,");
    writer.Write("Электронная почта 2,");
    writer.Write("Номер телефона 1,");
    writer.Write("Номер телефона 2\n");

    //проходимся по каждой компании
    foreach (CompanyClass company in json?.companies) {
        //если в компании нет проектов то пропускаем
        if (company.projects == null) continue;
        //проходимся по каждому проекту компании
        foreach (ProjectClass project in company.projects) {
            //заполняем поля
            writer.Write(id + ", ");
            writer.Write(Format(company.inn) + ", ");
            writer.Write(Format(company.orn) + ", ");
            writer.Write(Format(company.ogrn) + ", ");
            writer.Write(Format(company.full_name?.ru) + ", ");
            writer.Write(Format(project.name_ru) + ", ");
            writer.Write(Format(project.description_ru) + ", ");
            writer.Write(Format(company.description?.ru) + ", ");
            writer.Write(Format(company.founders?[0]) + ", ");
            writer.Write(Format(company.site) + ", ");
            writer.Write(Format(company.company_cluster?.name_ru) + ", ");
            writer.Write(Format(company.email?.Length > 0 ? company.email?[0] : null) + ", ");
            writer.Write(Format(company.email?.Length > 1 ? company.email?[1] : null) + ", ");
            writer.Write(Format(company.phones?.Length > 0 ? company.phones?[0] : null) + ", ");
            writer.Write(Format(company.phones?.Length > 1 ? company.phones?[1] : null) + "\n");
            id++;
        }
    }
    writer.Close();
}

//запуск
Start();
Console.ReadKey();
//инн, орн, юр.лицо, название стартапа, описание стартапа, описание компании, фио директора, сайт, отрасль, контакты (общие), электронная почта, телефон, другое

//дальше идут классы, в них содержатся поля для преобразования из JSON строки в полноценные объекты, в будущем если захочешь парсить другие данные 
//смотри на название поля в JSON тексте и переписывай его точь в точь
public class ResponseJsonCompanyCountClass {
    public int? company_count {get; set;}
}

public class ResponseJsonClass {
    public string? key {get; set;}
    public int? page {get; set;}
    public int? total_pages {get; set;}

    public string[]? sort {get; set;}
    public CompanyClass[]? companies {get; set;}
}

public class CompanyClass {
    public int? id {get; set;}
    public LangClass? full_name {get; set;}
    public LangClass? description {get; set;}
    public string[]? email {get; set;}
    public string? orn {get; set;}
    public string? ogrn {get; set;}
    public string? inn {get; set;}
    public string[]? founders {get; set;}
    public CompanyClusterClass? company_cluster {get; set;}
    public ProjectClass[]? projects {get; set;}
    public string? site {get; set;}
    public string[]? phones {get; set;}
}
public class CompanyClusterClass {
    public string? name_ru {get; set;}
}

public class LangClass {
    public string? ru {get; set;}
}

public class StaffClass {
    public string? fio_ru {get; set;}
}

public class ProjectClass {
    public string? name_ru {get; set;}
    public string? description_ru {get; set;}
}