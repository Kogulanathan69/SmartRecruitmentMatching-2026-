(()=>{
  const $=id=>document.getElementById(id);
  const params=new URLSearchParams(location.search);
  function message(t="",c=""){const e=$("message");if(!e)return;e.textContent=t;e.className=`status-message${c?` ${c}`:""}`}
  function otpMessage(t="",c=""){const e=$("otpMessage");if(!e)return;e.textContent=t;e.className=`status-message${c?` ${c}`:""}`}
  function busy(f,v){const b=f?.querySelector('button[type="submit"]');if(!b)return;if(v){b.dataset.old=b.textContent;b.disabled=true;b.textContent="Please waitâ€¦"}else{b.disabled=false;b.textContent=b.dataset.old||b.textContent}}
  async function submit(f,job,onError=message){busy(f,true);onError();try{await job()}catch(e){onError(e.message||"Something went wrong.","error")}finally{busy(f,false)}}
  function route(d){NexHireApi.saveAuth(d);location.replace(NexHireSession.homeForRole(d.role))}
  if(NexHireSession.isAuthenticated()&&/\/(login|register)\.html$/.test(location.pathname))NexHireSession.goHome();

  const login=$("loginForm");
  if(login){
    if(params.get("reason")==="session")message("Your session ended. Sign in to continue.","error");
    if(params.get("verified")==="1")message("Email verified. Sign in to continue.","success");
    login.addEventListener("submit",e=>{e.preventDefault();submit(login,async()=>{
      try{route(await NexHireApi.request("/api/Auth/login",{method:"POST",auth:false,body:{email:$("email").value.trim(),password:$("password").value}}))}
      catch(err){
        if(/not verified|OTP/i.test(err.message||"")){location.replace(`register.html?verify=${encodeURIComponent($("email").value.trim())}`);return}
        throw err;
      }
    })})
  }

  const reg=$("registerForm"),otpForm=$("otpForm");
  let pendingEmail=params.get("verify")||"";
  function showOtp(email,notice){
    pendingEmail=(email||"").trim().toLowerCase();
    if(reg)reg.hidden=true;
    if(otpForm)otpForm.hidden=false;
    if($("otpEmail"))$("otpEmail").textContent=pendingEmail;
    if($("otpCode"))$("otpCode").focus();
    otpMessage(notice||"Enter the OTP from your email.","success");
  }
  if(reg&&otpForm&&pendingEmail)showOtp(pendingEmail,"Enter the OTP previously sent to your email, or request a new code.");

  if(reg)reg.addEventListener("submit",e=>{e.preventDefault();submit(reg,async()=>{
    const r=document.querySelector('input[name="role"]:checked');if(!r)throw new Error("Choose whether you are looking for work or hiring.");
    const email=$("email").value.trim();
    const d=await NexHireApi.request("/api/Auth/register",{method:"POST",auth:false,body:{firstName:$("firstName").value.trim(),lastName:$("lastName").value.trim(),email,phoneNumber:$("phone").value.trim()||null,role:r.value,password:$("password").value,confirmPassword:$("confirmPassword").value}});
    showOtp(d.email||email,d.message);
    startResendCooldown(60);
  })});

  if(otpForm)otpForm.addEventListener("submit",e=>{e.preventDefault();submit(otpForm,async()=>{
    const otp=$("otpCode").value.trim();
    if(!/^\d{6}$/.test(otp))throw new Error("Enter the 6-digit OTP.");
    const d=await NexHireApi.request("/api/Auth/verify-otp",{method:"POST",auth:false,body:{email:pendingEmail,otp}});
    otpMessage(d.message||"Email verified successfully.","success");
    setTimeout(()=>location.replace("login.html?verified=1"),600);
  },otpMessage)});

  let resendTimer=null;
  function startResendCooldown(seconds){
    const b=$("resendOtpButton");if(!b)return;
    if(resendTimer)clearInterval(resendTimer);
    let left=seconds;b.disabled=true;b.textContent=`Resend in ${left}s`;
    resendTimer=setInterval(()=>{left--;if(left<=0){clearInterval(resendTimer);resendTimer=null;b.disabled=false;b.textContent="Resend code"}else b.textContent=`Resend in ${left}s`},1000);
  }
  const resend=$("resendOtpButton");
  if(resend)resend.addEventListener("click",async()=>{
    if(!pendingEmail){otpMessage("Email address is missing. Register again.","error");return}
    const old=resend.textContent;resend.disabled=true;resend.textContent="Sendingâ€¦";otpMessage();
    try{const d=await NexHireApi.request("/api/Auth/resend-otp",{method:"POST",auth:false,body:{email:pendingEmail}});otpMessage(d.message,"success");startResendCooldown(60)}
    catch(e){otpMessage(e.message||"Could not resend OTP.","error");resend.disabled=false;resend.textContent=old}
  });

  const forgot=$("forgotForm");if(forgot)forgot.addEventListener("submit",e=>{e.preventDefault();submit(forgot,async()=>{const d=await NexHireApi.request("/api/Auth/forgot-password",{method:"POST",auth:false,body:{email:$("email").value.trim()}});message(d.resetTokenForDevelopmentOnly?`${d.message} Development token: ${d.resetTokenForDevelopmentOnly}`:d.message,"success")})});
  const reset=$("resetForm");if(reset)reset.addEventListener("submit",e=>{e.preventDefault();submit(reset,async()=>{const d=await NexHireApi.request("/api/Auth/reset-password",{method:"POST",auth:false,body:{token:$("token").value.trim(),newPassword:$("newPassword").value,confirmNewPassword:$("confirmPassword").value}});message(d.message||"Password reset successful.","success")})});
  document.querySelectorAll("[data-password-toggle]").forEach(b=>b.addEventListener("click",()=>{const i=$(b.dataset.passwordToggle);const show=i.type==="password";i.type=show?"text":"password";b.textContent=show?"Hide":"Show";b.setAttribute("aria-pressed",String(show))}));
})();