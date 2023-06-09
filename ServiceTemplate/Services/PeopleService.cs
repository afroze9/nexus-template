﻿using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using LanguageExt.Common;
using {{RootNamespace}}.Abstractions;
using {{RootNamespace}}.Data;
using {{RootNamespace}}.DTO;
using {{RootNamespace}}.Entities;
using {{RootNamespace}}.Exceptions;

namespace {{RootNamespace}}.Services;

public class PeopleService : IPeopleService
{
    private readonly IMapper _mapper;
    private readonly UnitOfWork _unitOfWork;
    private readonly ILogger<PeopleService> _logger;
    private readonly IValidator<Person> _personValidator;

    public PeopleService(
        IMapper mapper,
        UnitOfWork unitOfWork,
        ILogger<PeopleService> logger,
        IValidator<Person> personValidator)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _personValidator = personValidator;
    }

    public async Task<List<PersonDto>> GetAllAsync()
    {
        List<Person> people = await _unitOfWork.People.AllAsync();
        return _mapper.Map<List<PersonDto>>(people);
    }

    public async Task<Result<Person>> CreateAsync(Person person)
    {
        ValidationResult validationResult = await _personValidator.ValidateAsync(person);

        if (!validationResult.IsValid)
        {
            return new Result<Person>(new ValidationException(validationResult.Errors));
        }
        
        if (await _unitOfWork.People.ExistsWithEmailAsync(person.Email))
        {
            return new Result<Person>(new AnotherPersonExistsWithSameEmailException(person.Email));
        }
        
        try
        {
            _unitOfWork.BeginTransaction();
            _unitOfWork.People.Add(person);
            _unitOfWork.Commit();

            return person;
        }
        catch (Exception ex)
        {
            CreatePersonException personException = new (ex);   
            _logger.LogInformation(EventIds.CreatePersonTransactionError, personException, CreatePersonException.ExceptionMessage);
            _unitOfWork.Rollback();
            return new Result<Person>(personException);
        }
    }

    public async Task<Result<PersonDto>> GetByIdAsync(int id)
    {
        Person? person = await _unitOfWork.People.GetByIdAsync(id);
        
        if (person == null)
        {
            return new Result<PersonDto>(new PersonNotFoundException(id));
        }
        
        return _mapper.Map<PersonDto>(person);
    }

    public async Task<Result<Person>> UpdateNameAsync(int id, string name)
    {
        Person? personToUpdate = await _unitOfWork.People.GetByIdAsync(id);

        if (personToUpdate == null)
        {
            return new Result<Person>(new PersonNotFoundException(id));
        }
        
        personToUpdate.UpdateName(name);
        ValidationResult? validationResult = await _personValidator.ValidateAsync(personToUpdate);
        if (!validationResult.IsValid)
        {
            return new Result<Person>(new ValidationException(validationResult.Errors));
        }
        
        _unitOfWork.BeginTransaction();
        _unitOfWork.Commit();

        return personToUpdate;
    }

    public async Task DeleteAsync(int id)
    {
        Person? person = await _unitOfWork.People.GetByIdAsync(id);

        if (person == null)
        {
            return;
        }
        
        _unitOfWork.BeginTransaction();
        _unitOfWork.People.Delete(person);
        _unitOfWork.Commit();
    }
}