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
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = _userRepository.GetByUserName(username);
            if (user == null) return NotFound();

            // TÍNH LẠI SỐ DƯ THỰC TẾ
            var revenueShares = await _revenueShareRepository.GetByUserNameAsync(username);
            var approvedWithdrawals = await _withdrawalRequestRepository.GetApprovedByUserNameAsync(username);

            var totalRevenue = revenueShares.Sum(rs => rs.Amount);
            var totalWithdrawn = approvedWithdrawals.Sum(w => w.Amount);
            var actualBalance = totalRevenue - totalWithdrawn;

            // Cập nhật lại số dư (tùy chọn nếu bạn muốn lưu vào DB)
            user.WalletBalance = actualBalance;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();

            var viewModel = new WalletViewModel
            {
                User = user,
                RevenueShares = revenueShares,
                WithdrawalRequests = await _withdrawalRequestRepository.GetByUserNameAsync(username)
            };

            return View(viewModel);
        }

        // GET: Wallet/Withdraw
        public async Task<IActionResult> Withdraw()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = _userRepository.GetByUserName(username);
            if (user == null) return NotFound();

            // TÍNH LẠI SỐ DƯ
            var revenue = await _revenueShareRepository.GetByUserNameAsync(username);
            var withdrawn = await _withdrawalRequestRepository.GetApprovedByUserNameAsync(username);
            var balance = revenue.Sum(r => r.Amount) - withdrawn.Sum(w => w.Amount);

            var viewModel = new WithdrawRequestViewModel
            {
                UserName = username,
                CurrentBalance = balance
            };

            return View(viewModel);
        }

        // POST: Wallet/Withdraw
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(WithdrawRequestViewModel model)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            var user = _userRepository.GetByUserName(username);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return NotFound();
            }

            // TÍNH LẠI SỐ DƯ THỰC TẾ
            var revenueShares = await _revenueShareRepository.GetByUserNameAsync(username);
            var approvedWithdrawals = await _withdrawalRequestRepository.GetApprovedByUserNameAsync(username);
            var actualBalance = revenueShares.Sum(rs => rs.Amount) - approvedWithdrawals.Sum(w => w.Amount);

            model.CurrentBalance = actualBalance;
            model.UserName = username;

            // VALIDATE
            if (model.Amount > actualBalance)
            {
                ModelState.AddModelError("Amount", $"Số tiền rút không được vượt quá số dư: {actualBalance:N0} ₫");
            }

            if (model.Amount < 1000)
            {
                ModelState.AddModelError("Amount", "Số tiền rút tối thiểu là 1.000 ₫.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // TẠO YÊU CẦU RÚT TIỀN
            var withdrawalRequest = new WithdrawalRequest
            {
                WithdrawalRequestId = "WR" + DateTime.Now.Ticks.ToString().Substring(10),
                UserName = username,
                Amount = model.Amount,
                BankName = model.BankName?.Trim(),
                AccountNumber = model.AccountNumber?.Trim(),
                AccountHolderName = string.IsNullOrEmpty(model.AccountHolderName?.Trim())
                    ? user.FullName
                    : model.AccountHolderName.Trim(),
                Status = "Pending",
                RequestDate = DateTime.Now
            };

            try
            {
                await _withdrawalRequestRepository.CreateAsync(withdrawalRequest);
                TempData["Success"] = "Yêu cầu rút tiền đã được gửi thành công! Vui lòng chờ duyệt.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                return View(model);
            }
        }

        // GET: Wallet/WithdrawalHistory
        public async Task<IActionResult> WithdrawalHistory()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");

            var requests = await _withdrawalRequestRepository.GetByUserNameAsync(username);
            return View(requests);
        }

        // GET: Wallet/RevenueHistory
        public async Task<IActionResult> RevenueHistory()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");

            var shares = await _revenueShareRepository.GetByUserNameAsync(username);
            return View(shares);
        }
    }

    // ViewModels
    public class WalletViewModel
    {
        public User User { get; set; } = null!;
        public IEnumerable<RevenueShare> RevenueShares { get; set; } = Enumerable.Empty<RevenueShare>();
        public IEnumerable<WithdrawalRequest> WithdrawalRequests { get; set; } = Enumerable.Empty<WithdrawalRequest>();
    }

    public class WithdrawRequestViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền")]
        [Range(1000, double.MaxValue, ErrorMessage = "Số tiền tối thiểu là 1.000 ₫")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên ngân hàng")]
        [StringLength(100)]
        public string BankName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số tài khoản")]
        [StringLength(20)]
        public string AccountNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? AccountHolderName { get; set; }
    }
}