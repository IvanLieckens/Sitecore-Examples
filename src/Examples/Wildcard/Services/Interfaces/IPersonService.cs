using System.Collections.Generic;

using Examples.Areas.Wildcard.Models;

namespace Examples.Wildcard.Services.Interfaces
{
    public interface IPersonService
    {
        IEnumerable<Person> GetAll(out int totalResults, int page = 0, int pageSize = 20);

        IEnumerable<Person> GetByName(string name);
    }
}
