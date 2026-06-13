using ERPWebApp.Data;
using ERPWebApp.Models;
using ERPWebApp.Models.Company.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Controllers.Company.Security;

[Authorize(Roles = RoleList.Administrator + "," + RoleList.HRManager + "," + RoleList.HRBasic + "," + RoleList.SecurityBasic + "," + RoleList.SecurityManager + "," + RoleList.Developer)]
[AutoValidateAntiforgeryToken]
public class SecurityAccessController : Controller
{
    private readonly ApplicationDbContext _context;
    private static SecurityAccess securityAccess;
    private readonly DateTime now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
    private const string password = "AccessCardEncryption";

    public SecurityAccessController(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> IndexAsync()
    {
        securityAccess = new SecurityAccess();
        securityAccess.AccessPlans = await _context.AccessPlan.OrderBy(x => x.AccessPlanName).ToListAsync();
        securityAccess.AccessPoints = await _context.AccessPoint.OrderBy(x => x.AccessPointLocation).ToListAsync();
        securityAccess.AccessCards = await _context.AccessCard.Include(a => a.Employee).Include(b => b.Employee.Department).Select(a => new AccessCard
        {
            AccessCardId = a.AccessCardId,
            Employee = a.Employee,
            EmployeeId = a.EmployeeId,
            Key = SecurityEncryption.Decrypt(a.Key, password)
        }).OrderBy(x => x.Employee.FirstName).ToListAsync();
        securityAccess.AccessPlanDoors = await _context.AccessPlanDoor.Include(a => a.AccessPlan).Include(a => a.AccessPoint).OrderBy(x => x.AccessPoint.AccessPointLocation).ToListAsync();
        securityAccess.AccessPlanUsers = await _context.AccessPlanUser.Include(a => a.AccessPlan).Include(a => a.AccessCard).OrderBy(x => x.AccessCard.Employee.FirstName).ToListAsync();
        securityAccess.AccessPlanUsers.ForEach(x => x.AccessCard.Key = SecurityEncryption.Decrypt(x.AccessCard.Key, password));
        ViewBag.AccessPlanCount = securityAccess.AccessPlans.Count;
        ViewBag.AccessPlanIds = securityAccess.AccessPlans.Select(x => x.AccessPlanId).ToList();
        return View(securityAccess);
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public List<int> GetAccessPlanIds()
    {
        return _context.AccessPlan.Select(x => x.AccessPlanId).ToList();
    }

    public async Task<AccessPlanDoor> SaveAccessPlanDoorMapping(int accessPlanId, int accessPointId)
    {
        AccessPlanDoor newPlanDoor = new AccessPlanDoor()
        {
            AccessPlanId = accessPlanId,
            AccessPlan = _context.AccessPlan.Single(x => x.AccessPlanId == accessPlanId),
            AccessPointId = accessPointId,
            AccessPoint = _context.AccessPoint.Single(x => x.AccessPointId == accessPointId),
            CreatedBy = User.Identity.Name,
            CreationDate = now
        };

        _context.AccessPlanDoor.Add(newPlanDoor);
        securityAccess.AccessPlanDoors.Add(newPlanDoor);
        await _context.SaveChangesAsync();
        return newPlanDoor;
    }

    public async Task<AccessPoint> DeleteAccessPlanDoorMapping(int accessPlanId, int accessPointId)
    {
        var PlanDoorToRemove = securityAccess.AccessPlanDoors.Single(e => e.AccessPlanId == accessPlanId && e.AccessPointId == accessPointId);
        _context.AccessPlanDoor.Remove(PlanDoorToRemove);
        securityAccess.AccessPlanDoors.Remove(PlanDoorToRemove);
        await _context.SaveChangesAsync();
        return securityAccess.AccessPoints.Single(e => e.AccessPointId == accessPointId);
    }


    public async Task<AccessPlanUser> SaveAccessPlanUserMapping(int accessPlanId, int accessCardId)
    {
        AccessPlanUser newPlanUser = new AccessPlanUser()
        {
            AccessPlanId = accessPlanId,
            AccessPlan = _context.AccessPlan.Single(x => x.AccessPlanId == accessPlanId),
            AccessCardId = accessCardId,
            AccessCard = _context.AccessCard.Single(x => x.AccessCardId == accessCardId),
            CreatedBy = User.Identity.Name,
            CreationDate = now
        };
        _context.AccessPlanUser.Add(newPlanUser);
        securityAccess.AccessPlanUsers.Add(newPlanUser);
        await _context.SaveChangesAsync();
        return newPlanUser;
    }

    public async Task<AccessCard> DeleteAccessPlanUserMapping(int accessPlanId, int accessCardId)
    {
        var PlanUserToRemove = securityAccess.AccessPlanUsers.Single(e => e.AccessPlanId == accessPlanId && e.AccessCardId == accessCardId);
        _context.AccessPlanUser.Remove(PlanUserToRemove);
        securityAccess.AccessPlanUsers.Remove(PlanUserToRemove);
        await _context.SaveChangesAsync();
        return securityAccess.AccessCards.Single(e => e.AccessCardId == accessCardId);
    }

    public async Task<int> SaveKeyAccessLog(AccessPointLog accessPointLog)
    {
        _context.Add(accessPointLog);
        return await _context.SaveChangesAsync();
    }


    [HttpGet()]
    [ProducesResponseType(200)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [AllowAnonymous]
    //https://localhost:44363/SecurityAccess/KeyCardAccess/?uid=6154&key=04F19DCAF26C80&receivedPassword=GZ05fonjJs/WTrwXeiBhTg7G9IH805JyE/k1GpoV/sJHEGo0ZpRvsc8FbS0t12ejY4x0iEeR48HUMRdirtmW4Q==&macAddress=E0:E2:E6:D0:95:30&ipAddress=192.168.80.59
    public async Task<IActionResult> KeyCardAccessAsync(string receivedPassword, [FromBody] string uId, string key, string macAddress, string ipAddress)
    {
        //Check Encrypted Password to begin validation process
        string decryptedPassword;
        AccessCard entryCard = new AccessCard();
        AccessPointLog accessPointLog = new AccessPointLog()
        {
            RecievedUID = uId,
            RecievedKey = key,
            RecievedPassword = receivedPassword,
            RecievedMacAddress = macAddress,
            RecievedIpAddress = ipAddress,
            CreationDate = now
        };

        try
        {
            //await SaveKeyAccessLog(accessPointLog);
            decryptedPassword = SecurityEncryption.Decrypt(receivedPassword, password);
            if (decryptedPassword != password)
                return BadRequest("Wrong Password.");
            var entryCards = await _context.AccessCard.Include(a => a.Employee).Include(b => b.Employee.Department).ToListAsync();
            if (entryCards.Any(e => SecurityEncryption.Decrypt(e.Key, decryptedPassword) == key))
                entryCard = entryCards.Single(e => SecurityEncryption.Decrypt(e.Key, decryptedPassword) == key);
        }
        catch (Exception ex)
        {
            await SaveKeyAccessLog(accessPointLog);
            return BadRequest(ex);
        }

        if (uId == null || entryCard.Key == null || key == null || macAddress == null)
        {
            await SaveKeyAccessLog(accessPointLog);
            return BadRequest(new { errorMessage = "Incomplete Data or Unassigned Card." });
        }

        var entryPoint = _context.AccessPoint.Single(e => e.MacAddress == macAddress);
        accessPointLog.AccessCardId = entryCard.AccessCardId;
        accessPointLog.AccessPointId = entryPoint.AccessPointId;

        //Values need to be valid and need to exist
        //Need to decrypt existing key because the encryption I use generates a different encripted version 
        if (!_context.AccessPoint.Any(e => e.MacAddress == macAddress))
        {
            accessPointLog.IsSuccess = false;
            await SaveKeyAccessLog(accessPointLog);
            return NotFound("Access Point is not found.");
        }


        var entryEmployee = entryCard.Employee;
        //Terminated Employees cannot enter no matter what
        if (entryEmployee.JobStatus == Models.Company.JobStatus.Terminated)
        {
            accessPointLog.IsSuccess = false;
            await SaveKeyAccessLog(accessPointLog);
            return BadRequest("Employee is marked as Terminated.");
        }


        //If an access point is open then anyone can enter
        if (entryPoint.Status == AccessPointStatus.Open)
        {
            accessPointLog.IsSuccess = true;
            await SaveKeyAccessLog(accessPointLog);
            return Ok(1);
        }

        List<AccessPlanDoor> planDoors = await _context.AccessPlanDoor.Include(a => a.AccessPlan).Include(a => a.AccessPoint).Where(e => e.AccessPointId == entryPoint.AccessPointId).ToListAsync();
        List<AccessPlanUser> planUsers = await _context.AccessPlanUser.Include(a => a.AccessPlan).Include(a => a.AccessCard).Where(e => e.AccessCardId == entryCard.AccessCardId).ToListAsync();

        if (planDoors.Count() == 0)
        {
            accessPointLog.IsSuccess = false;
            await SaveKeyAccessLog(accessPointLog);
            return BadRequest("The Access Point is not assigned to an Access Plan.");
        }

        if (planUsers.Count() == 0)
        {
            accessPointLog.IsSuccess = false;
            await SaveKeyAccessLog(accessPointLog);
            return BadRequest("The Access Card is not assigned to an Access Plan.");
        }

        if (entryPoint.IpAddress != ipAddress)
        {
            entryPoint.IpAddress = ipAddress;
            _context.AccessPoint.Update(entryPoint);
            await _context.SaveChangesAsync();
        }

        accessPointLog.IsSuccess = true;
        await SaveKeyAccessLog(accessPointLog);
        return Ok(1);
    }

    [HttpGet()]
    [ProducesResponseType(200)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [AllowAnonymous]
    public async Task<IActionResult> GetKeyCardTotal(string receivedPassword)
    {
        //Check Encrypted Password to begin validation process
        string decryptedPassword;
        decryptedPassword = SecurityEncryption.Decrypt(receivedPassword, password);
        if (decryptedPassword != password)
            return BadRequest("Wrong Password.");
        try
        {
            int totalKeyCards = await _context.AccessCard.CountAsync();
            return Ok(totalKeyCards);
        }
        catch (Exception ex)
        {
            return BadRequest("DB Error: " + ex.Message);
        }

    }


    public string EncryptPhrase(string key, bool? ToEncrypt = true)
    {
        if (ToEncrypt == true)
            return SecurityEncryption.Encrypt(key, password);
        else
            return SecurityEncryption.Decrypt(key, password);
    }



}
