using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class WalletController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IRevenueShareRepository _revenueShareRepository;
        private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;

        public WalletController(
            IUserRepository userRepository,
            IRevenueShareRepository revenueShareRepository,
            IWithdrawalRequestRepository withdrawalRequestRepository)
        {
            _userRepository = userRepository;
            _revenueShareRepository = revenueShareRepository;
            _withdrawalRequestRepository = withdrawalRequestRepository;
        }

        // GET: Wallet/Index
        public async Task<IActionResult> Index()
        {
            var username = User.Identity.Name;
            var user = _userRepository.GetByUserName(username);

            if (user == null)
            {
                return NotFound();
            }

            // Lấy lịch sử chia doanh thu
            var revenueShares = await _revenueShareRepository.GetByUserNameAsync(username);

            // Lấy lịch sử yêu cầu rút tiền
            var withdrawalRequests = await _withdrawalRequestRepository.GetByUserNameAsync(username);

            var viewModel = new WalletViewModel
            {
                User = user,
                RevenueShares = revenueShares,
                WithdrawalRequests = withdrawalRequests
            };

            return View(viewModel);
        }

        // GET: Wallet/Withdraw
        public IActionResult Withdraw()
        {
            var username = User.Identity.Name;
            var user = _userRepository.GetByUserName(username);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new WithdrawRequestViewModel
            {
                UserName = username,
                CurrentBalance = user.WalletBalance
            };

            return View(viewModel);
        }

        // POST: Wallet/Withdraw
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(WithdrawRequestViewModel model)
        {
            var username = User.Identity.Name;
            var user = _userRepository.GetByUserName(username);

            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra số dư
            if (model.Amount > user.WalletBalance)
            {
                ModelState.AddModelError("Amount", "Số tiền rút không được vượt quá số dư hiện tại.");
            }

            if (model.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Số tiền rút phải lớn hơn 0.");
            }

            if (ModelState.IsValid)
            {
                var withdrawalRequest = new WithdrawalRequest
                {
                    WithdrawalRequestId = Guid.NewGuid().ToString(),
                    UserName = username,
                    Amount = model.Amount,
                    BankName = model.BankName,
                    AccountNumber = model.AccountNumber,
                    AccountHolderName = model.AccountHolderName,
                    Status = "Pending",
                    RequestDate = DateTime.Now
                };

                await _withdrawalRequestRepository.CreateAsync(withdrawalRequest);

                TempData["Success"] = "Yêu cầu rút tiền đã được gửi thành công! Vui lòng chờ admin duyệt.";
                return RedirectToAction("Index");
            }

            model.CurrentBalance = user.WalletBalance;
            return View(model);
        }

        // GET: Wallet/WithdrawalHistory
        public async Task<IActionResult> WithdrawalHistory()
        {
            var username = User.Identity.Name;
            var withdrawalRequests = await _withdrawalRequestRepository.GetByUserNameAsync(username);

            return View(withdrawalRequests);
        }

        // GET: Wallet/RevenueHistory
        public async Task<IActionResult> RevenueHistory()
        {
            var username = User.Identity.Name;
            var revenueShares = await _revenueShareRepository.GetByUserNameAsync(username);

            return View(revenueShares);
        }
    }

    public class WalletViewModel
    {
        public User User { get; set; }
        public IEnumerable<RevenueShare> RevenueShares { get; set; }
        public IEnumerable<WithdrawalRequest> WithdrawalRequests { get; set; }
    }

    public class WithdrawRequestViewModel
    {
        public string UserName { get; set; }
        public decimal CurrentBalance { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập số tiền")]
        [Range(1000, double.MaxValue, ErrorMessage = "Số tiền tối thiểu là 1,000 VNĐ")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên ngân hàng")]
        [StringLength(100, ErrorMessage = "Tên ngân hàng không được vượt quá 100 ký tự")]
        public string BankName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tài khoản")]
        [StringLength(20, ErrorMessage = "Số tài khoản không được vượt quá 20 ký tự")]
        public string AccountNumber { get; set; }

        [StringLength(100, ErrorMessage = "Tên chủ tài khoản không được vượt quá 100 ký tự")]
        public string? AccountHolderName { get; set; }
    }
}
