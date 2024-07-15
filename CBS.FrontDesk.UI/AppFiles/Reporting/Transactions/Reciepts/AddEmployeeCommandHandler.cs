
using MediatR;
using AutoMapper;
using CBS.CUSTOMER.COMMON.UnitOfWork;
using CBS.CUSTOMER.DOMAIN.Context;
using Microsoft.Extensions.Logging;
using CBS.CUSTOMER.REPOSITORY;
using Microsoft.EntityFrameworkCore;
using CBS.CUSTOMER.HELPER.Helper;
using CBS.CUSTOMER.DATA.Dto;
using CBS.Customer.MEDIATR;

namespace CBS.Employee.MEDIATR.EmployeeMediatR.Handlers
{
    /// <summary>
    /// Handles the command to add a new Employee.
    /// </summary>
    public class AddEmployeeCommandHandler : IRequestHandler<AddEmployeeCommand, ServiceResponse<CreateEmployee>>
    {
        private readonly IEmployeeRepository _EmployeeRepository; // Repository for accessing Employee data.
        private readonly IMapper _mapper; // AutoMapper for object mapping.
        private readonly ILogger<AddEmployeeCommandHandler> _logger; // Logger for logging handler actions and errors.
        private readonly IUnitOfWork<POSContext> _uow;
        /// <summary>
        /// Constructor for initializing the AddEmployeeCommandHandler.
        /// </summary>
        /// <param name="EmployeeRepository">Repository for Employee data access.</param>
        /// <param name="mapper">AutoMapper for object mapping.</param>
        /// <param name="claimTypeRepository">Repository for ClaimType data access.</param>
        /// <param name="logger">Logger for logging handler actions and errors.</param>
        public AddEmployeeCommandHandler(
            IEmployeeRepository EmployeeRepository,
            IMapper mapper,
            ILogger<AddEmployeeCommandHandler> logger,
            IUnitOfWork<POSContext> uow)
        {
            _EmployeeRepository = EmployeeRepository;
            _mapper = mapper;
            _logger = logger;
            _uow = uow;
        }

        /// <summary>
        /// Handles the AddEmployeeCommand to add a new Employee.
        /// </summary>
        /// <param name="request">The AddEmployeeCommand containing Employee data.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task<ServiceResponse<CreateEmployee>> Handle(AddEmployeeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                /*// Check if a Employee with the same name already exists (case-insensitive)
                var existingEmployee = await _EmployeeRepository.FindBy(c => c.E == (request.EmployeeName)).FirstOrDefaultAsync();

                // If a Employee with the same name already exists, return a conflict response
                if (existingEmployee != null)
                {
                    var errorMessage = $"Employee {(request.EmployeeName)} already exists.";
                    _logger.LogError(errorMessage);
                    return ServiceResponse<CreateEmployee>.Return409(errorMessage);
                }*/

                // Map the AddEmployeeCommand to a Employee entity
                var EmployeeEntity = _mapper.Map<CUSTOMER.DATA.Entity.Employee>(request);

               

                EmployeeEntity.CreatedDate = BaseUtilities.UtcToLocal(DateTime.UtcNow);

                //Auto generateKey For Unique Identification
                EmployeeEntity.EmployeeId = BaseUtilities.GenerateUniqueNumber();
 
                // Add the new Employee entity to the repository
                _EmployeeRepository.Add(EmployeeEntity);
                if (await _uow.SaveAsync() <= 0)
                {
                    return ServiceResponse<CreateEmployee>.Return500();
                }
                // Map the Employee entity to CreateEmployee and return it with a success response
                var CreateEmployee = _mapper.Map<CreateEmployee>(EmployeeEntity);
                return ServiceResponse<CreateEmployee>.ReturnResultWith200(CreateEmployee);
            }
            catch (Exception e)
            {
                // Log error and return 500 Internal Server Error response with error message
                var errorMessage = $"Error occurred while saving Employee: {e.Message}";
                _logger.LogError(errorMessage);
                return ServiceResponse<CreateEmployee>.Return500(e);
            }
        }
    }

}
