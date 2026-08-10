# 🎯 Today's Goal

After today's session, Dhruv will be able to design, declare, and invoke clean, single-responsibility C# methods with proper parameter passing and return handling. He will understand how methods execute on the call stack internally and be ready to apply functional decomposition in real-world backend architectures.

---

# 📘 Core Concept

Methods are named code blocks that encapsulate a specific sequence of statements to perform a dedicated task. They solve code duplication (violating the DRY principle) and high cognitive load by breaking complex application workflows into small, reusable unit operations.

### How It Works Internally
When a method is called, the C# runtime (.NET CLR) allocates a **Stack Frame** on the execution stack. This frame stores:
1. The arguments passed into the method.
2. The method's local variables.
3. The return address (where execution resumes after the method finishes).

When execution hits a `return` statement or the end of a `void` method, the stack frame is popped, memory used by local variables is reclaimed immediately, and control returns to the caller.

```
+--------------------------+
| ProcessOrder Stack Frame | <-- Active frame (Local variables, Params)
+--------------------------+
| Main Stack Frame         | <-- Waiting frame (Return address)
+--------------------------+
```

### Key Rules & Terminologies
* **Method Signature:** Defined strictly by the **Method Name** and the **Type and Order of its Parameters**. *The return type is NOT part of the signature.*
* **Access Modifiers:** Control visibility (`public`, `private`, `internal`, `protected`). Default inside classes is `private`.
* **Value vs. Reference Types:** By default, parameters pass by value (copies of data for value types, copies of reference pointers for reference types). Keywords like `in`, `ref`, and `out` change this behavior.
* **Expression-Bodied Syntax:** Uses `=>` (lambda arrow) for single-line returns to reduce boilerplate.

### What Happens If You Do It Wrong
* **Stack Overflow Exception:** Excessive recursion or infinite method calls deplete stack memory.
* **Tightly Coupled Code:** Methods that perform multiple unrelated operations (e.g., validation, database writing, email sending in one block) make unit testing impossible and introduce regressions.

### Complete Runnable Code Example

```csharp
using System;

namespace MethodBasics
{
    class Program
    {
        static void Main(string[] args)
        {
            decimal itemPrice = 100.00m;
            decimal taxRate = 0.18m;

            // Invoking a method with return value
            decimal finalPrice = CalculateTotal(itemPrice, taxRate);
            
            // Invoking an expression-bodied method
            PrintReceipt("Order #1001", finalPrice);
        }

        // Standard method returning a decimal value
        public static decimal CalculateTotal(decimal price, decimal tax)
        {
            if (price <= 0) return 0m; // Guard clause
            return price + (price * tax);
        }

        // Expression-bodied method returning void
        public static void PrintReceipt(string orderId, decimal amount) 
            => Console.WriteLine($"[{orderId}] Total Due: ${amount:F2}");
    }
}
```

---

# 💼 Real Project Example

In production web applications built with ASP.NET Core, methods must maintain clear isolation, perform input validation via guard clauses, and leverage Dependency Injection.

### Business Scenario
An order processing service needs a method to compute user discounts based on customer tier before persisting the transaction to a database.

```csharp
using System;

namespace ECommerce.Services
{
    public interface IDiscountService
    {
        decimal ApplyTierDiscount(decimal totalAmount, string customerTier);
    }

    public class DiscountService : IDiscountService
    {
        public decimal ApplyTierDiscount(decimal totalAmount, string customerTier)
        {
            // Guard clauses prevent processing invalid input
            if (totalAmount <= 0)
                throw new ArgumentOutOfRangeException(nameof(totalAmount), "Amount must be positive.");

            if (string.IsNullOrWhiteSpace(customerTier))
                return totalAmount;

            decimal discountPercentage = GetDiscountPercentage(customerTier);
            return totalAmount - (totalAmount * discountPercentage);
        }

        // Private helper method isolating tier lookup logic
        private decimal GetDiscountPercentage(string tier) => tier.ToLower() switch
        {
            "gold" => 0.20m,
            "silver" => 0.10m,
            "bronze" => 0.05m,
            _ => 0.00m
        };
    }
}
```

### How It Works & Architecture Insights
1. **Separation of Concerns:** `ApplyTierDiscount` handles validation and application, delegating discount lookup logic to `GetDiscountPercentage`.
2. **Guard Clauses:** Throwing `ArgumentOutOfRangeException` early stops invalid data execution before calculating results.
3. **Senior Perspective:** Senior developers prefer keeping methods pure (no global state modification) and private for logic confined to one class. If `GetDiscountPercentage` grows to need a database lookup, it will be refactored into a separate injected repository method.

---

# ⚠️ Top 3 Mistakes

### 1. Violating Single Responsibility ("God Methods")
**Bad Code:**
```csharp
public void ProcessUserRegistration(string username, string email)
{
    // Validate email, format string, save to DB, log event, send SMTP email all in one method
    if (!email.Contains("@")) return;
    // ... 80 lines of DB and Email code ...
}
```
**Why It Fails:** Untestable, prone to unexpected side-effects, and breaking one part breaks the entire registration workflow.
**Correct Fix:**
```csharp
public void ProcessUserRegistration(string username, string email)
{
    ValidateInput(username, email);
    SaveToDatabase(username, email);
    SendWelcomeEmail(email);
}
```

---

### 2. Overusing `out` Parameters Instead of Tuples or Objects
**Bad Code:**
```csharp
public bool GetUserData(int userId, out string name, out string email, out int age)
{
    // Sets multiple out variables
    name = "Dhruv"; email = "dhruv@test.com"; age = 25;
    return true;
}
```
**Why It Fails:** `out` parameters make methods hard to read, uncomposable, and force awkward caller syntax.
**Correct Fix:**
```csharp
// Use C# Tuples or Records
public (string Name, string Email, int Age) GetUserData(int userId)
{
    return ("Dhruv", "dhruv@test.com", 25);
}
```

---

### 3. Missing Guard Clauses (Deeply Nested If-Else Statements)
**Bad Code:**
```csharp
public decimal CalculateBonus(User user)
{
    if (user != null) {
        if (user.IsActive) {
            return user.Sales * 0.1m;
        } else {
            return 0m;
        }
    }
    return 0m;
}
```
**Why It Fails:** Deep nesting increases cognitive complexity and makes edge cases easy to miss during code reviews.
**Correct Fix:**
```csharp
public decimal CalculateBonus(User user)
{
    if (user == null || !user.IsActive) return 0m;
    return user.Sales * 0.1m;
}
```

---

# 📰 Industry News

- **Stripe Uses Graph Search and State Machines to Automate Database Remediation**
  Stripe implemented graph search algorithms and formal state machines to safely automate database issue resolution at scale. Understanding clear, deterministic state transitions inside modular service methods is critical when building automated infrastructure logic.
  [Read full article](https://www.infoq.com/news/2026/08/database-remediation-graph/?utm_campaign=infoq_content&utm_source=infoq&utm_medium=feed&utm_term=global)

- **Presentation: Keeping ChatGPT Fast as AI Development Accelerates**
  OpenAI engineers detail performance engineering strategies to minimize latency during agentic coding executions. Writing lean, low-allocation C# methods directly impacts execution speeds when processing high-volume streaming backend workloads.
  [Read full article](https://www.infoq.com/presentations/openai-performance-engineering-agentic-coding/?utm_campaign=infoq_content&utm_source=infoq&utm_medium=feed&utm_term=global)

- **Cloudflare's Precursor Detects Bots and AI Agents through Continuous Behavioral Analysis**
  Cloudflare's new engine analyzes real-time user action flows to differentiate human activity from automated AI agents. Backend logic relies on single-purpose methods to parse high-frequency signal events without introducing processing bottlenecks.
  [Read full article](https://www.infoq.com/news/2026/08/cloudflare-precursor-detection/?utm_campaign=infoq_content&utm_source=infoq&utm_medium=feed&utm_term=global)

- **GitHub Hardens npm and Actions Defaults, Drawing Debate over Delays versus Signing**
  GitHub updated security defaults for Actions and package registries to combat supply chain vulnerabilities. As security hardens across build pipelines, writing deterministic and easily unit-testable methods ensures security checks pass smoothly.
  [Read full article](https://www.infoq.com/news/2026/08/github-npm-actions-defaults/?utm_campaign=infoq_content&utm_source=infoq&utm_medium=feed&utm_term=global)

- **Cloudflare Launches Persistent, Stateful, Computer-Like Environments for Agents**
  Cloudflare has made stateful environments accessible for AI agents to perform complex multi-step computations. Designing stateless, pure methods allows agentic frameworks to call API endpoints reliably across variable execution runtimes.
  [Read full article](https://www.infoq.com/news/2026/08/cloudflare-computer-agents/?utm_campaign=infoq_content&utm_source=infoq&utm_medium=feed&utm_term=global)

- **Instacart Builds Blueberry, an AI-Powered Assistant to Help On-Call Engineers Investigate Incidents**
  Instacart created an automated SRE assistant to diagnose production outages rapidly by parsing stack traces. Clean stack traces generated by well-named, small methods significantly accelerate automated incident analysis during outages.
  [Read full article](https://www.infoq.com/news/2026/08/instacart-blueberry-sre-ai/?utm_campaign=infoq_content&utm_source=infoq&utm_medium=feed&utm_term=global)

- **AI Is Transforming Incident Response - But the Hardest Problems May Still Belong to Humans**
  While AI tools quickly surface incident diagnostics, core architectural problem-solving remains human-driven. Clean software design fundamentals—like cohesive method structure—remain essential for human engineers maintaining complex systems.
  [Read full article](https://www.infoq.com/news/2026/08/ai-incident-response/?utm_campaign=infoq_content&utm_source=infoq&utm_medium=feed&utm_term=global)

---

# ❓ Interview Questions & Answers

**Q1: What is a method signature in C#, and does the return type form part of it?**

**A1:** A method signature uniquely identifies a method to the compiler and consists only of the **method name**, **parameter types**, and **parameter modifiers (`ref`, `out`, `in`)**. The **return type is NOT part of the signature**.

```csharp
// Signature: Process(int)
public int Process(int value) => value * 2;

// CAUSES COMPILER ERROR: Same signature as above!
// public string Process(int value) => value.ToString();
```

---

**Q2: What is the difference between passing arguments by value vs. passing with `ref` or `out`?**

**A2:** By default, arguments are passed **by value** (a copy is created). The `ref` keyword passes a reference to the variable, requiring initialization before passing. The `out` keyword passes by reference for output purposes and requires the called method to assign a value before returning.

```csharp
void Modify(ref int x, out int y) 
{
    x += 10;
    y = 50; // Must be assigned inside method
}
```

---

**Q3: How do expression-bodied methods differ from block-bodied methods?**

**A3:** Expression-bodied methods use the `=>` operator to define short, single-expression methods. They are purely syntactic sugar and compile down to the exact same IL instructions as block-bodied methods `{ return ...; }`.

```csharp
// Expression-bodied
public int Square(int x) => x * x;

// Equivalent Block-bodied
public int SquareBlock(int x) { return x * x; }
```

---

**Q4: What is method overloading, and how does the C# compiler resolve overloaded calls?**

**A4:** Method overloading allows multiple methods in the same scope to share the same name if their parameter signatures differ. The C# compiler uses **compile-time overload resolution** to select the method with the best match based on argument counts, types, and implicit conversions.

```csharp
public class Logger {
    public void Log(string text) => Console.WriteLine(text);
    public void Log(Exception ex) => Console.WriteLine(ex.Message);
}
```

---

**Q5: What happens at the memory level when a method is invoked in C#?**

**A5:** Upon invocation, C# allocates a dedicated frame on the **Call Stack** storing input arguments, local variables, and the execution return address. When method execution terminates, this frame is immediately popped off, instantly releasing stack memory without triggering the Garbage Collector.

```csharp
public void Execute() 
{
    int localVal = 10; // Allocated on the current call stack frame
} // Stack frame popped here
```

---

**Q6: What are local functions in C#, and how do they differ from private class methods?**

**A6:** Local functions are private methods nested directly inside another parent method. Unlike class methods, they can access local variables within the parent scope (closure) and are scoped strictly to the enclosing block, preventing unintended usage elsewhere in the class.

```csharp
public int CalculateFactorial(int n)
{
    return LocalFactorial(n);
    
    // Local function hidden from the rest of the class
    int LocalFactorial(int x) => x <= 1 ? 1 : x * LocalFactorial(x - 1);
}
```

---

# 📚 Revision Summary

### Day 3: Control Statements
Control statements (`if/else`, `switch`, `for`, `foreach`, `while`) direct application execution flow.
* **Key Idea:** Branching evaluates conditions to skip or execute blocks; loops repeat actions over collections or until conditions met.
* **One Thing to Remember:** Favor pattern-matching `switch` expressions and early exit guard clauses to keep code flat and avoid deep indentation nesting.

### Day 1: Variables and Data Types
Data types declare the kind of values stored in variables, split into **Value Types** (stored on stack) and **Reference Types** (stored on heap).
* **Key Idea:** Strongly typed variables ensure type safety at compile time, reducing runtime evaluation crashes.
* **One Thing to Remember:** Primitive numeric types (like `int`, `decimal`) copy values directly on assignment, whereas object and class references copy memory pointers pointing to heap addresses.

---

# 🚀 Tomorrow Preview

Tomorrow, Dhruv will explore **Method Overloading & Parameter Modifiers (`params`, `optional`, `named arguments`)**. He will learn how to design flexible method APIs that adapt to variable caller requirements without creating redundant code duplication.