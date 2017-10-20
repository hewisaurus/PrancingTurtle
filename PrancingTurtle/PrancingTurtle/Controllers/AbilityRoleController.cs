using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Database.Models;
using Database.Repositories.Interfaces;
using PagedList;
using PrancingTurtle.Helpers.Authorization;
using PrancingTurtle.Models.AbilityRole;

namespace PrancingTurtle.Controllers
{
    [CustomAuthorization]
    public class AbilityRoleController : Controller
    {
        private readonly IAbilityRoleRepository _repository;
        private readonly IPlayerClassRepository _playerClass;
        private readonly IRoleIconRepository _roleIcon;

        public AbilityRoleController(IAbilityRoleRepository repository, IPlayerClassRepository playerClass, IRoleIconRepository roleIcon)
        {
            _repository = repository;
            _playerClass = playerClass;
            _roleIcon = roleIcon;
        }

        
        public async Task<ActionResult> Index(string so, string an, string aid, string soul, int? cid, int? rid, int ps = 10, int pn = 1)
        {
            var viewModel = new AbilityRoleIndexViewModel(so,an, aid, soul, cid, rid, ps, pn);
            return View("Index", await ProcessAbilityRoleIndexViewModel(viewModel));
        }

        [HttpPost]
        
        public async Task<ActionResult> Index(AbilityRoleIndexViewModel viewModel)
        {
            viewModel.Page = 1;
            return View("Index", await ProcessAbilityRoleIndexViewModel(viewModel));
        }

        private async Task<AbilityRoleIndexViewModel> ProcessAbilityRoleIndexViewModel(AbilityRoleIndexViewModel viewModel)
        {
            var filters = viewModel.Filters;

            var records = _repository.GetPagedData(filters, viewModel.DatabaseSortBy, viewModel.Page, viewModel.PageSize);

            viewModel.SetSortParameters();

            viewModel.PagedItems = new StaticPagedList<AbilityRole>
                (records.Data, viewModel.Page, viewModel.PageSize, records.TotalRecords);

            var classes = await _playerClass.GetAllAsync();
            classes.Insert(0, null);
            viewModel.Classes = classes;

            var roles = await _roleIcon.GetAllAsync();
            roles.Insert(0, null);
            viewModel.Roles = roles;

            return viewModel;
        }
    }
}