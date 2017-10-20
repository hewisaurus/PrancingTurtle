using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Database.Models;
using Database.Repositories.Interfaces;
using PagedList;
using PrancingTurtle.Helpers.Authorization;
using PrancingTurtle.Models.ViewModels;
using Ability = Database.Models.Ability;

namespace PrancingTurtle.Controllers
{
    [CustomAuthorization]
    public class AbilityController : Controller
    {
        private readonly IAbilityRepository _ability;

        public AbilityController(IAbilityRepository ability)
        {
            _ability = ability;
        }

        
        public ActionResult Index(string so, string sn, string sa, string ssp,int? sci, int? ssi, int ps = 10, int pn = 1)
        {
            var model = new AbilityIndexVm(so, sn, sa, ssp, sci, ssi, ps, pn);
            return View("Index", ProcessIndexViewModel(model));
        }
        [HttpPost]
        public ActionResult Index(AbilityIndexVm viewModel)
        {
            return View("Index", ProcessIndexViewModel(viewModel));
        }

        private AbilityIndexVm ProcessIndexViewModel(AbilityIndexVm viewModel)
        {
            #region Filters
            var filters = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(viewModel.SearchName)) { filters.Add(viewModel.GetDatabaseColumnName("Name"), viewModel.SearchName); }
            if (!string.IsNullOrEmpty(viewModel.SearchAbilityId)) { filters.Add(viewModel.GetDatabaseColumnName("Ability"), long.Parse(viewModel.SearchAbilityId)); }
            //if (!string.IsNullOrEmpty(viewModel.SearchRank)) { filters.Add(viewModel.GetDatabaseColumnName("Rank"), int.Parse(viewModel.SearchRank)); }
            //if (!string.IsNullOrEmpty(viewModel.SearchIcon)) { filters.Add(viewModel.GetDatabaseColumnName("Icon"), viewModel.SearchIcon); }
            if (!string.IsNullOrEmpty(viewModel.SearchSoulPoints)) { filters.Add(viewModel.GetDatabaseColumnName("SoulPoints"), int.Parse(viewModel.SearchSoulPoints)); }
            //if (viewModel.SearchClassId != null) { filters.Add(viewModel.GetDatabaseColumnName("Class"), viewModel.SearchClassId); }
            //if (viewModel.SearchSoulId != null) { filters.Add(viewModel.GetDatabaseColumnName("Soul"), viewModel.SearchSoulId); }
            #endregion
            //var abilities = _repository.GetPagedAbilities(filters, viewModel.DatabaseSortBy, viewModel.Page, viewModel.PageSize);
            //TODO: make this use the ability repository and the GetPagedData method
            var abilities = _ability.GetPagedData(filters, viewModel.DatabaseSortBy, viewModel.Page, viewModel.PageSize);

            //List<Soul> souls = _repository.GetSouls().ToList();
            //souls.Insert(0, null);
            //viewModel.Souls = souls;
            viewModel.Souls = new List<Soul>();

            //List<PlayerClass> classes = _repository.GetClasses().ToList();
            //classes.Insert(0, null);
            //viewModel.Classes = classes;
            viewModel.Classes = new List<PlayerClass>();

            viewModel.SetSortParameters();

            viewModel.PagedItems = new StaticPagedList<Ability>(abilities.Data, viewModel.Page, viewModel.PageSize,
                abilities.TotalRecords);

            return viewModel;
        }
    }
}