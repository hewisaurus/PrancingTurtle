using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Database.Models;
using Database.Repositories.Interfaces;
using PrancingTurtle.Helpers.Authorization;
using PrancingTurtle.Helpers.Controllers;
using PrancingTurtle.Helpers.DataTables;
using Logging;

namespace PrancingTurtle.Controllers
{
    [CustomAuthorization]
    public class BossFightController : BaseController
    {
        private readonly IBossFightRepository _repository;
        private readonly ILogger _logger;
        private readonly IInstanceRepository _instance;

        public BossFightController(IBossFightRepository bossFight, ILogger logger, IInstanceRepository instance)
        {
            _repository = bossFight;
            _logger = logger;
            _instance = instance;
        }

        // GET: BossFight
        public async Task<ActionResult> Index()
        {
            var allBossFights = await _repository.GetAllAsync();
            return View(allBossFights);
        }


        public async Task<ActionResult> Create()
        {
            var model = new BossFight
            {
                Instances = await _instance.GetAllAsync()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Create(BossFight model)
        {
            if (!ModelState.IsValid)
            {
                model.Instances = await _instance.GetAllAsync();
                return View(model);
            }

            var result = await _repository.Create(model);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                model.Instances = await _instance.GetAllAsync();
                return View(model);
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index");
            }
            var model = await _repository.GetAsync(id);
            model.Instances = await _instance.GetAllAsync();

            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> Edit(BossFight model)
        {
            if (!ModelState.IsValid)
            {
                model.Instances = await _instance.GetAllAsync();
                return View(model);
            }

            var result = await _repository.Update(model);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                model.Instances = await _instance.GetAllAsync();
                return View(model);
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index");
            }
            var result = await _repository.GetAsync(id);
            if (result == null)
            {
                return RedirectToAction("Index");
            }
            return View(result);
        }
        [HttpPost]
        public async Task<ActionResult> Delete(BossFight model)
        {
            var res = await _repository.Delete(model.Id);
            if (res.Success)
            {
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", res.Message);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> LoadBossFights()
        {
            var dtBuilder = new CustomDataTableBuilder(new CustomDataTableParam(Request.Form));
            dtBuilder.BuildFilterAndOrder(CurrentController, CurrentAction);
            var pagedData = await _repository.GetPagedDataAsync(dtBuilder.Filters, dtBuilder.OrderBy, dtBuilder.Parameters.Skip, dtBuilder.Parameters.PageSize, true);
            return Json(new { draw = dtBuilder.Parameters.Draw, recordsFiltered = pagedData.TotalRecords, recordsTotal = pagedData.TotalRecords, data = pagedData.Data }, JsonRequestBehavior.AllowGet);
        }
    }
}