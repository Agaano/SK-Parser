using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

string FindInnerString(string str, string start, string end) {
    int startIndex = str.IndexOf(start);
    if (startIndex == -1) 
        return "";
    string restString = str[(startIndex + start.Length)..];
    int endIndex = restString.IndexOf(end);
    if (endIndex == -1) 
        return "";
    string innerString = restString[0..endIndex];
    // Console.WriteLine($"End Index: {endIndex} \nEnd Char: {str[endIndex]} \nStart Index: {startIndex} \nStart Char: {str[startIndex]} \nInnerString: {innerString}");
    return innerString;
} 

string[] FindAllInnerStrings(string str, string start, string end) {
    string st = str;
    string innerString = "11";
    List<string> strings = new();
    while (innerString.Length > 0) {
        innerString = FindInnerString(st, start, end);
        if (innerString.Length > 0) strings.Add(innerString);
        st = st[st.IndexOf(innerString)..];
    }
    return [.. strings];
}

string Format(string? str) {
    return $"\"{str?.Replace("\n", "").Replace("\r", "").Replace("\"", "\"\"")}\"";
}

async void Start() {
    HttpClient client = new();
    HttpResponseMessage responseMessage = await client.GetAsync("https://navigator.sk.ru/navigator/api/overall/company_stat");
    ResponseJsonCompanyCountClass? response1 = JsonSerializer.Deserialize<ResponseJsonCompanyCountClass>(await responseMessage.Content.ReadAsStreamAsync());
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
    requestMessage.Headers.Add("Cookie", "navigator_session=eyJfZnJlc2giOmZhbHNlLCJ1c2VyX2lkIjowfQ.Zq1vfA.84AqtPTFijINEGUM2FnoSC51gRg; navigator_session=eyJfZnJlc2giOmZhbHNlLCJ1c2VyX2lkIjowfQ.Zq1m2A.JAi9wQ0xCzujJftPkl0Tip9Oa1c; navigator_session_logs=c789d6021eddb5247f11226f4c007d3c; sk_lang=ru");
    HttpResponseMessage response = await client.SendAsync(requestMessage);
    string stream = await response.Content.ReadAsStringAsync();
    ResponseJsonClass? json = JsonSerializer.Deserialize<ResponseJsonClass>(stream);
    Console.WriteLine(stream);
    int id = 1;
    using StreamWriter writer = new("./table.csv");
    writer.Write("Номер, ");
    writer.Write("ИНН, ");
    writer.Write("ОРН, ");
    writer.Write("Юр.лицо, ");
    writer.Write("Название стартапа, ");
    writer.Write("Описание стартапа, ");
    writer.Write("Описание компании, ");
    writer.Write("ФИО Директора, ");
    writer.Write("Сайт, ");
    writer.Write("Отрасль, ");
    writer.Write("Контакты \n");
    foreach (CompanyClass company in json?.companies) {
        if (company.projects == null) continue;
        foreach (ProjectClass project in company.projects) {
            writer.Write(id + ", ");
            writer.Write(Format(company.inn) + ", ");
            writer.Write(Format(company.orn) + ", ");
            writer.Write(Format(company.full_name?.ru) + ", ");
            writer.Write(Format(project.name_ru) + ", ");
            writer.Write(Format(project.description_ru) + ", ");
            writer.Write(Format(company.description?.ru) + ", ");
            writer.Write(Format(company.founders?[0]) + ", ");
            writer.Write(Format(company.site) + ", ");
            writer.Write(Format(company.company_cluster?.name_ru) + ", ");
            writer.Write(Format(company.email?.Length > 0 ? company.email?[0] : null) + "\n");
            id++;
        }
    }
    writer.Close();
}


Start();
Console.ReadKey();
//инн, орн, юр.лицо, название стартапа, описание стартапа, описание компании, фио директора, сайт, отрасль, контакты (общие), электронная почта, телефон, другое

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
    public string? inn {get; set;}
    public string[]? founders {get; set;}
    public CompanyClusterClass? company_cluster {get; set;}
    public ProjectClass[]? projects {get; set;}
    public string? site {get; set;}
    
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