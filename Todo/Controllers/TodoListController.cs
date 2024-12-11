using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Todo.Data;
using Todo.Data.Entities;
using Todo.EntityModelMappers.TodoLists;
using Todo.Models.TodoLists;
using Todo.Services;

namespace Todo.Controllers
{
    [Authorize]
    public class TodoListController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IUserStore<IdentityUser> userStore;

        public TodoListController(ApplicationDbContext dbContext, IUserStore<IdentityUser> userStore)
        {
            this.dbContext = dbContext;
            this.userStore = userStore;
        }

        public IActionResult Index()
        {
            var userId = User.Id();
            var todoLists = dbContext.RelevantTodoLists(userId);
            var viewmodel = TodoListIndexViewmodelFactory.Create(todoLists);
            return View(viewmodel);
        }

        public IActionResult Detail(int todoListId, string sortBy = null, string order = "desc")
        {
            var todoList = dbContext.SingleTodoList(todoListId);

            IEnumerable<TodoItem> items = todoList.Items;
            sortBy = sortBy ?? "importance";
            if (sortBy == "importance")
            {
                var importanceOrder = new[] { Importance.High, Importance.Medium, Importance.Low };
                items = order == "asc"
                    ? items.OrderByDescending(item => Array.IndexOf(importanceOrder, item.Importance))
                    : items.OrderBy(item => Array.IndexOf(importanceOrder, item.Importance));
            }
            else if (sortBy == "rank")
            {
                items = order == "asc" ? items.OrderByDescending(item => item.Rank) : items.OrderBy(item => item.Rank);
            }

            todoList.Items = items.ToList();

            var viewmodel = TodoListDetailViewmodelFactory.Create(todoList);
            return View(viewmodel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new TodoListFields());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TodoListFields fields)
        {
            if (!ModelState.IsValid) { return View(fields); }

            var currentUser = await userStore.FindByIdAsync(User.Id(), CancellationToken.None);

            var todoList = new TodoList(currentUser, fields.Title);

            await dbContext.AddAsync(todoList);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Create", "TodoItem", new {todoList.TodoListId});
        }
    }
}