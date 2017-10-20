using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Database.Repositories.Interfaces;
using Logging;
using Database.Models;
using PrancingTurtle.Helpers.Authorization;

namespace PrancingTurtle.Controllers
{
    [CustomAuthorization]
    public class InstanceController : Controller
    {
        private readonly IInstanceRepository _repository;
        private readonly ILogger _logger;

        public InstanceController(IInstanceRepository instance ,ILogger logger)
        {
            _repository = instance;
            _logger = logger;
        }

        // GET: Instance
        //[VisitorTracker]
        public async Task<ActionResult> Index()
        {
            var allInstances = await _repository.GetAllAsync();
            return View(allInstances);
        }

       
        public async Task<ActionResult> Create()
        {
            // create model
            var model = new Instance
            {
                // set any default model properties

            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Create(Instance model)
        {
            // check model state exists
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            // async new instance object to the db
            var result = await _repository.Create(model);

            // check if successful and return to appropriate view/error message
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
       
        public async Task<ActionResult> Edit(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index");
            }
            var model = await _repository.GetAsync(id); 
            
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> Edit(Instance model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var result = await _repository.Update(model);
            // check result is successful
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
       
        public async Task<ActionResult> Delete(int id = -1)
        {
            if (id == -1)
            {
                return RedirectToAction("Index");
            }
            var res = await _repository.GetAsync(id);
            if (res == null)
            {
                return RedirectToAction("Index");
            }
            return View(res);
        }
        [HttpPost]
        public async Task<ActionResult> Delete(Instance model)
        {
            var res = await _repository.Delete(model.Id);
            if (res.Success)
            {
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", res.Message);
            return View(model);
        }
    }
}