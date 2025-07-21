using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RewardsAndRecognitionRepository;
using RewardsAndRecognitionRepository.Interfaces;
using RewardsAndRecognitionRepository.Models;
using RewardsAndRecognitionRepository.Repos;
using RewardsAndRecognitionSystem.ViewModels;
using RewardsAndRecognitionRepository.Service;

namespace RewardsAndRecognitionSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUserRepo _userRepo;
        private readonly UserManager<User> _userManager;
        private readonly ITeamRepo _teamRepo;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public UserController(
            IMapper mapper
            IUserRepo userRepo, 
            UserManager<User> userManager, 
            ITeamRepo teamRepo, 
            ApplicationDbContext context,
            IEmailService emailService )
        {
            _mapper = mapper;
            _userRepo = userRepo;
            _userManager = userManager;
            _teamRepo = teamRepo;
            _context = context;
            _emailService = emailService;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
        .Include(u => u.Team)
            .ThenInclude(t => t.Manager)
        .ToListAsync();

            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "No Role";
            }
            ViewBag.UserRoles = userRoles;
            var usersList=_mapper.Map<List<UserViewModel>>(users);
            return View(usersList);
        }

        // GET: User/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropDowns();
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropDowns();
                return View(viewModel);
            }

            var user = _mapper.Map<User>(viewModel);
            if (ModelState.IsValid)
            {
                user.UserName = user.Email;
                user.NormalizedEmail = user.Email.ToUpper();
                user.EmailConfirmed = true;
                user.CreatedAt = DateTime.UtcNow;

                var result = await _userManager.CreateAsync(user, viewModel.PasswordHash);

                if (result.Succeeded)
                {
                    // Add user to selected role or default to Employee
                    var roleToAssign = viewModel.SelectedRole;
                    var roleResult = await _userManager.AddToRoleAsync(user, roleToAssign);
                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                            ModelState.AddModelError("", error.Description);
                        await PopulateDropDowns();
                        return View(user);
                    }

                    //string imgAddress = "https://phxonline-my.sharepoint.com/:i:/g/personal/myuvaraj_goud_zelis_com/EXr_YatQxidNn2RFFt3o-KEBuqUi54Vn0lVYm7Btlv62qg?e=zW1cqe";

                    await _emailService.SendEmailAsync(
                        subject: "🎉 Welcome to Rewards and Recognition!",
                        to: user.Email,
                        isHtml:true,
                        body: $@"
                        <body style=""font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #ffffff;"">
                          <div style=""background-color: #ffffff; padding: 10px 20px; max-width: 600px; margin: auto; color: #000;"">
                            <img src=""cid:bannerImage"" alt=""Zelis Banner"" style=""width: 100%; max-width: 600px;"">

                            <p>Dear <strong>{user.Name}</strong>,</p>

                            <p>Welcome to the <strong>Rewards and Recognition</strong> platform!</p>

                            <p>
                              Your account has been successfully created. Below are your login credentials:
                            </p>

                            <p style=""font-size: 16px;"">
                              <strong>Email:</strong> {user.Email}<br>
                              <strong>Temporary Password:</strong> <span style=""color: #black;"">{password}</span>
                            </p>

                            <p>
                              Please log in and <strong>change your password immediately</strong> to secure your account.
                            </p>

                            <p>
                              If you have any questions or need help accessing your account, feel free to contact our support team.
                            </p>

                            <p style=""font-size: 13px; color: #999; text-align: center;"">
                              — This email was sent from the <strong>Rewards & Recognition</strong> system
                            </p>
                          </div>
                        </body>
                        "
                    );


                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            //ModelState.Clear();
            await PopulateDropDowns();
            return View(viewModel);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var viewModel = _mapper.Map<UserViewModel>(user);

            await PopulateDropDowns();
            return View(viewModel);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserViewModel updatedUserViewModel)
        {
            if (!ModelState.IsValid)
            {
                var usermodel = await _userManager.FindByIdAsync(id);
                var userViewmodel=_mapper.Map<UserViewModel>(usermodel);
                ModelState.Clear();
                await PopulateDropDowns();
                return View(userViewmodel);
            }

            var updatedUser = _mapper.Map<User>(updatedUserViewModel);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            
            if (ModelState.IsValid)
            {
                user.NormalizedUserName = updatedUser.Email.ToUpper();
                user.UserName = updatedUser.Email;
                user.Name = updatedUser.Name;
                user.Email = updatedUser.Email;
                user.NormalizedEmail = updatedUser.Email.ToUpper();
                user.TeamId = updatedUser.TeamId;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            ModelState.Clear();
            await PopulateDropDowns();
            return View(user);
        }

     
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            return RedirectToAction(nameof(Index));

        }



        private async Task PopulateDropDowns()
        {
            var teams = await _teamRepo.GetAllAsync();
            var managers = await _userRepo.GetAllManagersAsync();
            var roles = new List<string> { "Admin", "TeamLead", "Manager", "Director", "Employee" };

            ViewBag.Teams = new SelectList(teams, "Id", "Name");
            ViewBag.Managers = new SelectList(managers, "Id", "Name");
            ViewBag.Roles = roles;
        }
    }
}
